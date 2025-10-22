using System.Diagnostics;
using Microsoft.Win32;

namespace PowerLab.UpdaterService
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Version currentVersion = GetInstalledVersion();
                _logger.LogInformation("当前版本: {version}", currentVersion);
                var newestVersion = GetLastestVersion();
                _logger.LogInformation("最新版本: {version}", newestVersion);
                if (newestVersion <= currentVersion)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }

                string? installFile =
                    Directory.GetFiles(Path.Combine(@"D:\Workspace\Personal\DNFPowerLabPackage", newestVersion.ToString()))
                             .FirstOrDefault();

                if (installFile is null)
                {
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }

                _logger.LogInformation("检查到新版本安装文件：{installFile}，准备安装。", installFile);

                await WaitAppExit();

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = installFile,
                    Arguments = "/quiet",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process process = Process.Start(processStartInfo);
                return;
            }
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
        private static Version GetLastestVersion()
        {
            string[] subDirList = Directory.GetDirectories(@"D:\Workspace\Personal\DNFPowerLabPackage");
            Version? newestVersion = subDirList.Max(path => Version.TryParse(Path.GetFileName(path), out var v) ? v : new Version("1.0.0"));
            return newestVersion ?? new Version("1.0.0");
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
