using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using AuraEcho.Installer.Bootstrapper.Constants;
using AuraEcho.PluginContracts.Constants;
using AuraEcho.PluginContracts.Interfaces;
using AuraEcho.PluginContracts.Models;
using AuraEcho.Setup.UI.Utils;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace AuraEcho.Installer.Bootstrapper.ViewModels;

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

        DirectoryInfo installFolder = new DirectoryInfo(GetInstallPath());
        List<Process> runningProcesses =
            [.. allProcesses.Where(p =>
            {
                string? exePath = p.GetExecutablePath();
                if (String.IsNullOrEmpty(exePath)) return false;

                DirectoryInfo processDir = new DirectoryInfo(Path.GetDirectoryName(exePath));
                return String.Equals(processDir.FullName, installFolder.FullName, StringComparison.OrdinalIgnoreCase);
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
                    Message = @"AuraEcho 仍在运行，正在等待 AuraEcho 退出，选择 ""继续"" 以退出 AuraEcho 继续安装。",
                    Title = "AuraEcho 仍在运行"
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
        const string keyPath = @"Software\AuraEcho";
        using RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
        if (key == null) return null;

        object value = key.GetValue("InstallPath");
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
