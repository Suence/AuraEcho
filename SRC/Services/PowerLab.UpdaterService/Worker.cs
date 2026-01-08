using System.Diagnostics;
using Microsoft.Win32;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;
using PowerLab.PluginContracts.Interfaces;

namespace PowerLab.UpdaterService;

public class Worker : BackgroundService
{
    private IAppLogger _logger;
    private IAppPackageRepository _packageRespository;
    private ILocalPluginRepository _localPluginRepository;
    private IRemotePluginRepository _remotePluginRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _basePath;
    private readonly string _appPackageCachePath;
    private readonly string _pluginPackageCachePath;
    private AppUpdateInfo _cachedAppUpdateInfo;
    private Dictionary<Guid, PluginUpdateInfo> _cachedPluginUpdateInfo = [];
    public Worker(IAppLogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "PowerLab", "UpdaterService", "Download");

        _appPackageCachePath = Path.Combine(_basePath, "PackageCache");
        _pluginPackageCachePath = Path.Combine(_basePath, "PluginCache");
        Directory.CreateDirectory(_basePath);
        Directory.CreateDirectory(_appPackageCachePath);
        Directory.CreateDirectory(_pluginPackageCachePath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("ExecuteAsync");

        using var scope = _serviceProvider.CreateScope();

        _packageRespository  = scope.ServiceProvider.GetRequiredService<IAppPackageRepository>();
        _localPluginRepository = scope.ServiceProvider.GetRequiredService<ILocalPluginRepository>();
        _remotePluginRepository = scope.ServiceProvider.GetRequiredService<IRemotePluginRepository>();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            await DownloadPackage();

            if (IsAppRunning()) continue;

            await InstallPackage();
        }
    }

    private async Task DownloadPackage()
    {
        await DownloadAppPackage();
        await DownloadPluginPackage();
    }

    private async Task DownloadPluginPackage()
    {
        _logger.Information("开始检测插件版本信息...");
        List<PluginRegistryModel> installedPlugins = _localPluginRepository.GetPluginRegistries();
        foreach (var plugin in installedPlugins)
        {
            var latestPackage = await _remotePluginRepository.GetLatestAsync(plugin.Manifest.Id);
            var latestVersion = latestPackage is null
                ? new Version("0.0.0")
                : new Version(latestPackage.Version);

            _logger.Information($"{plugin.Manifest.PluginName} 当前版本: {plugin.Manifest.Version}, 最新版本: {latestVersion}");

            var cachedVersion = _cachedPluginUpdateInfo.ContainsKey(plugin.Manifest.Id)
                ? new Version(_cachedPluginUpdateInfo[plugin.Manifest.Id].Version)
                : new Version("0.0.0");

            if (latestVersion <= new Version(plugin.Manifest.Version)) continue;

            if (latestVersion <= cachedVersion)
            {
                _logger.Information($"{plugin.Manifest.PluginName} {cachedVersion}已下载未安装，跳过下载");
                continue;
            }

            var targetPath = Path.Combine(_pluginPackageCachePath, latestPackage.FileName);
            bool result = await _remotePluginRepository.DownloadLatestAsync(plugin.Manifest.Id, "stable", targetPath, null);
            if (!result)
            {
                _logger.Information("插件安装包下载失败");
                continue;
            }
            _cachedPluginUpdateInfo[plugin.Manifest.Id] = new PluginUpdateInfo(plugin.Manifest.Id, latestPackage.Version, targetPath);
        }
    }

    private async Task DownloadAppPackage()
    {
        _logger.Information("开始检测客户端版本信息...");

        Version currentVersion = GetInstalledVersion();
        var newestVersion = await GetLastestVersionAsync();
        _logger.Information($"当前版本: {currentVersion}, 最新版本: {newestVersion.Version}");

        var newestVer = new Version(newestVersion.Version);
        var cachedVer = new Version(_cachedAppUpdateInfo?.Version ?? "0.0.0");
        if (newestVer <= currentVersion || newestVer <= cachedVer)
        {
            _logger.Information("未检测到新版本");
            return;
        }

        _logger.Information("正在下载新版本安装包");
        var targetPath = Path.Combine(_appPackageCachePath, newestVersion.FileName);
        var progress = new Progress<double>(p => { });
        bool result = await _packageRespository.DownloadLatestAsync("stable", targetPath, progress);
        _cachedAppUpdateInfo = new AppUpdateInfo(newestVersion.Version, targetPath);
    }

    private async Task InstallPackage()
    {
        await InstallPluginPackage();
        await InstallAppPackage();
    }

    private async Task InstallAppPackage()
    {
        if (_cachedAppUpdateInfo is null)
        {
            _logger.Information("没有新版本需要安装");
            return;
        }

        _logger.Information("开始启动客户端安装程序");
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _cachedAppUpdateInfo.FilePath,
            Arguments = "/quiet",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using Process? process = Process.Start(processStartInfo);
        if (process is not null)
        {
            await process.WaitForExitAsync();
            _logger.Information("客户端安装程序执行完成，继续检测更新。");
            File.Delete(_cachedAppUpdateInfo.FilePath);
            _cachedAppUpdateInfo = null;
            return;
        }
        _logger.Information("客户端安装程序启动失败");
    }

    private async Task InstallPluginPackage()
    {
        var cachedPluginIdList = _cachedPluginUpdateInfo.Keys.ToList();
        string? installFolder = Path.GetDirectoryName(GetInstallPath());
        if (installFolder is null)
        {
            _logger.Information("找不到客户端的安装目录");
            return;
        }

        string pluginInstallerPath = Path.Combine(installFolder, "PluginInstaller.exe");
        foreach (var pluginId in cachedPluginIdList)
        {
            var pluginUpdateInfo = _cachedPluginUpdateInfo[pluginId];
            _logger.Information($"开始安装插件 {pluginId} 的新版本 {pluginUpdateInfo.Version}");
            var processStartInfo = new ProcessStartInfo
            {
                FileName = pluginInstallerPath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            processStartInfo.ArgumentList.Add(pluginUpdateInfo.FilePath);
            processStartInfo.ArgumentList.Add("--nowindow");
            using Process? process = Process.Start(processStartInfo);
            if (process is not null)
            {
                await process.WaitForExitAsync();
                _logger.Information($"插件 {pluginId} 的安装程序执行完成。");
                File.Delete(pluginUpdateInfo.FilePath);
                _cachedPluginUpdateInfo.Remove(pluginId);
                continue;
            }
            _logger.Information($"插件 {pluginId} 的安装程序启动失败。");
        }
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("StopAsync");
        await base.StopAsync(cancellationToken);
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("StartAsync");
        await base.StartAsync(cancellationToken);
    }
    private static Version GetInstalledVersion()
    {
        const string keyPath = @"Software\PowerLab";
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
        if (key == null) return new Version("1.0.0");

        object? value = key.GetValue("InstallVersion");
        return new Version($"{value ?? "1.0.0"}");
    }
    private static string GetInstallPath()
    {
        const string keyPath = @"Software\PowerLab";
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
        if (key == null) return null;

        object? value = key.GetValue("InstallPath");
        return value?.ToString();
    }
    private async Task<AppVersionInfo> GetLastestVersionAsync()
    {
        var latestVersion = await _packageRespository.GetLatestAsync();
        return latestVersion ?? new AppVersionInfo { Version = "1.0.0" };
    }

    private static bool IsAppRunning()
    {
        List<Process> allProcesses =
        [.. Process.GetProcessesByName("PowerLab"),
         .. Process.GetProcessesByName("PlixInstaller")];

        if (allProcesses.Count <= 0) return false;

        string? installFolder = Path.GetDirectoryName(GetInstallPath());
        List<Process> runningProcesses =
            [.. allProcesses.Where(p => Path.GetDirectoryName(p.MainModule.FileName) == installFolder)];

        if (runningProcesses.Count <= 0) return false;

        return runningProcesses.Any(p => !p.HasExited);
    }
}

public record AppUpdateInfo(string Version, string FilePath);

public record PluginUpdateInfo(Guid PluginId, string Version, string FilePath);
