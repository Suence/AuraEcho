using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using PowerLab.Setup.UI.Utils;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.Installer.Bootstrapper.ViewModels;

public class UninstallPreparationViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private readonly IRegionDialogService _regionDialogService;

    public DelegateCommand NavigationToUninstallCommand { get; }
    private async void NavigationToUninstall()
    {
        if (!await StopAppAsync()) return;

        _regionManager.RequestNavigate(InstallerRegionNames.MainRegion, InstallerViewNames.Uninstalling);
    }

    private async Task<bool> StopAppAsync()
    {
        List<Process> allProcesses =
            [.. Process.GetProcessesByName(ProcessNames.HostProcess),
             .. Process.GetProcessesByName(ProcessNames.PluginInstaller)];

        if (allProcesses.Count <= 0) return true;

        string? installFolder = Path.GetDirectoryName(GetInstallPath());
        List<Process> runningProcesses =
            [.. allProcesses.Where(p =>
            {
                string? exePath = p.GetExecutablePath();
                if (string.IsNullOrEmpty(exePath)) return false;

                string? processDir = Path.GetDirectoryName(exePath);
                return String.Equals(processDir, installFolder, StringComparison.OrdinalIgnoreCase);
            })];

        if (runningProcesses.Count <= 0) return true;

        RegionDialogResult dialogResult =
            await _regionDialogService.ShowDialogAsync(
                InstallerRegionNames.MessageRegion,
                HostRegionDialogTypes.ConfirmDialog,
                new RegionDialogParameter
                {
                    CancelText = "重试",
                    ConfirmText = "继续",
                    Message = @"PowerLab 仍在运行，正在等待 PowerLab 退出，选择 ""继续"" 以退出 PowerLab 继续安装。",
                    Title = "PowerLab 仍在运行"
                });

        if (dialogResult == RegionDialogResult.Close) return false;

        if (dialogResult != RegionDialogResult.OK)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            return await StopAppAsync();
        }

        runningProcesses.ForEach(p => p.Kill());
        return true;
    }
    private static string GetInstallPath()
    {
        const string keyPath = @"Software\PowerLab";
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
        if (key == null) return null;

        object value = key.GetValue("LauncherPath");
        return value?.ToString();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="model"></param>
    public UninstallPreparationViewModel(IRegionManager regionManager, IRegionDialogService regionDialogService)
    {
        _regionManager = regionManager;
        _regionDialogService = regionDialogService;

        NavigationToUninstallCommand = new DelegateCommand(NavigationToUninstall);
    }
}
