using System.Diagnostics;
using Microsoft.Win32;
using PowerLab.UpdaterService.Contracts;
using PowerLab.UpdaterService.Models;

namespace PowerLab.UpdaterService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPackageRepository _packageRespository;
        private readonly string _basePath;
        private readonly string _appPackageCachePath;
        private readonly string _pluginPackageCachePath;
        private AppUpdateInfo _cachedAppUpdateInfo;

        public Worker(ILogger<Worker> logger, IPackageRepository packageRespository)
        {
            _logger = logger;
            _packageRespository = packageRespository;
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
            _logger.LogInformation("ExecuteAsync");
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
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        private async Task DownloadAppPackage()
        {
            _logger.LogInformation("开始检测客户端版本信息...");

            Version currentVersion = GetInstalledVersion();
            var newestVersion = await GetLastestVersionAsync();
            _logger.LogInformation("当前版本: {0}, 最新版本: {1}", currentVersion, newestVersion.Version);

            var newestVer = new Version(newestVersion.Version);
            var cachedVer = new Version(_cachedAppUpdateInfo?.Version ?? "0.0.0");
            if (newestVer <= currentVersion || newestVer <= cachedVer)
            {
                _logger.LogInformation("未检测到新版本");
                return;
            }

            _logger.LogInformation("正在下载新版本安装包");
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
                _logger.LogInformation("没有新版本需要安装");
                return;
            }

            _logger.LogInformation("开始启动客户端安装程序");
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
                _logger.LogInformation("客户端安装程序执行完成，继续检测更新。");
                _cachedAppUpdateInfo = null;
                return;
            }
            _logger.LogInformation("客户端安装程序启动失败");
        }

        private async Task InstallPluginPackage()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync");
            await base.StopAsync(cancellationToken);
        }
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartAsync");
            await base.StartAsync(cancellationToken);
        }
        private static Version GetInstalledVersion()
        {
            const string keyPath = @"Software\Suencesoft\PowerLab";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) return new Version("1.0.0");

            object? value = key.GetValue("InstallVersion");
            return new Version($"{value ?? "1.0.0"}");
        }
        private static string GetInstallPath()
        {
            const string keyPath = @"Software\Suencesoft\PowerLab";
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

    public record PluginUpdateInfo(string PluginId, string Version, string FilePath);
}
