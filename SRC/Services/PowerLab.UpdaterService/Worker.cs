using System.Diagnostics;
using Microsoft.Win32;
using PowerLab.UpdaterService.Contracts;
using PowerLab.UpdaterService.Models;

namespace PowerLab.UpdaterService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFileRespository _fileRespository;
        private readonly IVersionRespository _versionRespository;
        private readonly string _tempDownloadPath;
        public Worker(ILogger<Worker> logger, IVersionRespository versionRespository, IFileRespository fileRespository)
        {
            _logger = logger;
            _versionRespository = versionRespository;
            _fileRespository = fileRespository;

            _tempDownloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "PowerLab",
                "UpdaterService",
                "Temp");
            Directory.CreateDirectory(_tempDownloadPath);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("服务已启动");
            while (!stoppingToken.IsCancellationRequested)
            {
                Version currentVersion = GetInstalledVersion();
                _logger.LogInformation("当前版本: {version}", currentVersion);
                var newestVersion = await GetLastestVersionAsync();
                _logger.LogInformation("最新版本: {version}", newestVersion.Version);
                if (new Version(newestVersion.Version) <= currentVersion)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }
                _logger.LogInformation("正在下载新版本安装包");
                var targetPath = Path.Combine(_tempDownloadPath, $"{Guid.NewGuid()}.exe");
                var progress = new Progress<double>(p => _logger.LogInformation("下载进度: {progress}%", p.ToString("F2")));
                bool result = await _fileRespository.DownloadFileAsync(
                    newestVersion.DownloadUrl,
                    targetPath,
                    progress);

                if (!result)
                {
                    _logger.LogError("下载失败，稍后重试。");
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }
                _logger.LogInformation("下载完成：{setup_path}, 准备安装。", targetPath);

                await WaitAppExit();

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = targetPath,
                    Arguments = "/quiet",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process? process = Process.Start(processStartInfo);
                return;
            }
            _logger.LogInformation("ExecuteAsync END");
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StopAsync");
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
            var latestVersion = await _versionRespository.GetLatestAsync();
            return latestVersion ?? new AppVersionInfo { Version = "1.0.0" };
        }

        private async Task WaitAppExit()
        {
            List<Process> allProcesses =
            [.. Process.GetProcessesByName("PowerLab"),
             .. Process.GetProcessesByName("PlixInstaller")];

            if (allProcesses.Count <= 0) return;

            string? installFolder = Path.GetDirectoryName(GetInstallPath());
            List<Process> runningProcesses =
                [.. allProcesses.Where(p => Path.GetDirectoryName(p.MainModule.FileName) == installFolder)];

            if (runningProcesses.Count <= 0) return;

            foreach (Process process in runningProcesses)
            {
                _logger.LogInformation("正在等待 {processName} 进程退出", process.ProcessName);
                await process.WaitForExitAsync();
            }
        }
    }
}
