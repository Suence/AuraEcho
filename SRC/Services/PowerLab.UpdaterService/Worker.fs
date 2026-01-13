namespace PowerLab.UpdaterService

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open System.IO
open Microsoft.Extensions.DependencyInjection
open PowerLab.Core.Contracts
open PowerLab.PluginContracts.Interfaces
open Microsoft.Win32
open System.Diagnostics

type AppUpdateInfo = {
    Version: string
    FilePath: string
}

type PluginUpdateInfo = {
    PluginId: Guid
    Version: string
    FilePath: string
}

type Worker(logger: IAppLogger, serviceProvider: IServiceProvider) =
    inherit BackgroundService()
    
    let basePath = 
        Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.CommonApplicationData, 
                     "PowerLab", "UpdaterService", "Download")

    let appPackageCachePath = Path.Combine(basePath, "PackageCache")

    let pluginPackageCachePath = Path.Combine(basePath, "PluginCache")
    
    let mutable cachedAppUpdateInfo: AppUpdateInfo option = None
    let cachedPluginUpdateInfo = Dictionary<Guid, PluginUpdateInfo>()

    do
        [basePath; appPackageCachePath; pluginPackageCachePath]
        |> List.iter(fun path -> Directory.CreateDirectory path |> ignore)

    let getRegistryValue registryKey = 
        use key = Registry.LocalMachine.OpenSubKey @"Software\PowerLab"
        match key with
        | null -> None
        | _ -> key.GetValue registryKey |> Option.ofObj |> Option.map string

    let getInstallPath () = getRegistryValue "InstallPath"

    let getInstalledVersion () =
        getRegistryValue "InstallVersion"
        |> Option.defaultValue "1.0.0"
        |> Version

    let isAppRunning () =
        let installFolder = getInstallPath() |> Option.map Path.GetDirectoryName
        let processNames = ["PowerLab"; "PlixInstaller"]

        processNames
        |> List.collect (Process.GetProcessesByName >> List.ofArray)
        |> List.exists (fun p -> 
            try
                let pDir = Path.GetDirectoryName p.MainModule.FileName        
                installFolder = Some pDir && not p.HasExited    
            with _ -> false)

    let downloadAppPackage (packageRepo: IAppPackageRepository) = async {
        logger.Information "开始检测客户端版本信息..."
        let currentVersion = getInstalledVersion()
        let! latestInfo = packageRepo.GetLatestAsync() |> Async.AwaitTask
        let newestVersion = if isNull latestInfo then "1.0.0" else latestInfo.Version

        logger.Information $"当前版本: {currentVersion}, 最新版本: {newestVersion}"

        let newestVer = Version newestVersion
        let cachedVer = cachedAppUpdateInfo |> Option.map (fun i -> Version i.Version) |> Option.defaultValue (Version "0.0.0")

        if newestVer > currentVersion && newestVer > cachedVer then
            logger.Information "正在下载新版本安装包"
            let targetPath = Path.Combine(appPackageCachePath, latestInfo.FileName)
            let! success = packageRepo.DownloadLatestAsync("stable", targetPath, Progress<double> ignore) |> Async.AwaitTask
            if success then
                cachedAppUpdateInfo <- Some { Version = newestVersion; FilePath = targetPath }
        else
            logger.Information "未检测到新版本"
    }

    let downloadPluginPackage (localRepo: ILocalPluginRepository, remoteRepo: IRemotePluginRepository) = async {
        logger.Information "开始检测插件版本信息..."
        let installedPlugins = localRepo.GetPluginRegistries() |> List.ofSeq
        
        for plugin in installedPlugins do
            let! latestPackage = remoteRepo.GetLatestAsync plugin.Manifest.Id |> Async.AwaitTask
            let latestVersion = if isNull latestPackage then Version "0.0.0" else Version latestPackage.Version
            
            logger.Information $"{plugin.Manifest.PluginName} 当前版本: {plugin.Manifest.Version}, 最新版本: {latestVersion}"

            let cachedVersion = 
                match cachedPluginUpdateInfo.TryGetValue plugin.Manifest.Id with
                | true, info -> Version info.Version
                | _ -> Version "0.0.0"

            if latestVersion > Version plugin.Manifest.Version && latestVersion > cachedVersion then
                let targetPath = Path.Combine(pluginPackageCachePath, latestPackage.FileName)
                let! result = remoteRepo.DownloadLatestAsync(plugin.Manifest.Id, "stable", targetPath, null) |> Async.AwaitTask
                if result then
                    cachedPluginUpdateInfo.[plugin.Manifest.Id] <- { PluginId = plugin.Manifest.Id; Version = latestPackage.Version; FilePath = targetPath}
                else
                    logger.Information "插件安装包下载失败"
    }

    let installAppPackage () = async {
        match cachedAppUpdateInfo with
        | None -> logger.Information "没有新版本需要安装"
        | Some info ->
            logger.Information "开始启动客户端安装程序"
            let psi = ProcessStartInfo(info.FilePath, Arguments = "/quiet", UseShellExecute = false, CreateNoWindow = true)
            use p = Process.Start psi  
            if not (isNull p) then
                do! p.WaitForExitAsync() |> Async.AwaitTask
                logger.Information "客户端安装程序执行完成"
                File.Delete info.FilePath
                cachedAppUpdateInfo <- None
            else
                logger.Information "客户端安装程序启动失败"
            
    }

    let installPluginPackageCore cachedPluginIdList installFolder = task {
        let pluginInstallerPath = Path.Combine(installFolder, "PluginInstaller.exe")
        
        for pluginId in cachedPluginIdList do
            let info = cachedPluginUpdateInfo.[pluginId]
            logger.Information $"开始安装插件 {pluginId} 的新版本 {info.Version}"
            
            let psi = ProcessStartInfo(pluginInstallerPath, UseShellExecute = false, CreateNoWindow = true)
            psi.ArgumentList.Add info.FilePath
            psi.ArgumentList.Add "--nowindow"

            try
                use p = Process.Start psi
                if not (isNull p) then
                    do! p.WaitForExitAsync() |> Async.AwaitTask
                    
                    logger.Information $"插件 {pluginId} 的安装程序执行完成。"
                    if File.Exists info.FilePath then File.Delete info.FilePath
                    cachedPluginUpdateInfo.Remove pluginId |> ignore
                else
                    logger.Information $"插件 {pluginId} 的安装程序启动失败。"
            with ex ->
                logger.Information $"安装插件 {pluginId} 时发生异常: {ex.Message}"
    }

    let installPluginPackage () = async {
       
        let cachedPluginIdList = cachedPluginUpdateInfo.Keys |> Seq.toList
        let installPathOpt = getInstallPath() |> Option.map Path.GetDirectoryName

        match installPathOpt with
        | None -> logger.Information "找不到客户端的安装目录"
        | Some installFolder -> do! installPluginPackageCore cachedPluginIdList installFolder |> Async.AwaitTask
    }

    

    override _.ExecuteAsync(stoppingToken: CancellationToken) = task {
        logger.Information "ExecuteAsync 已启动"
        
        while not stoppingToken.IsCancellationRequested do
            try
                use scope = serviceProvider.CreateScope()
                let packageRepo = scope.ServiceProvider.GetRequiredService<IAppPackageRepository>()
                let localRepo = scope.ServiceProvider.GetRequiredService<ILocalPluginRepository>()
                let remoteRepo = scope.ServiceProvider.GetRequiredService<IRemotePluginRepository>()

                do! Task.Delay(TimeSpan.FromSeconds 10.0, stoppingToken)

                do! downloadAppPackage packageRepo |> Async.StartAsTask :> Task
                do! downloadPluginPackage (localRepo, remoteRepo) |> Async.StartAsTask :> Task

                if not (isAppRunning()) then
                    do! installPluginPackage () |> Async.StartAsTask :> Task
                    do! installAppPackage () |> Async.StartAsTask :> Task
                else
                    logger.Information "检测到程序正在运行，跳过安装阶段"

            with ex ->
                logger.Information $"工作循环中发生错误: {ex.Message}"
    }

    override _.StartAsync ct = 
        logger.Information "StartAsync"
        base.StartAsync ct

    override _.StopAsync ct = 
        logger.Information "StopAsync"
        base.StopAsync ct
