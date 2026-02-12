using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.WixToolset;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using PowerLab.Setup.UI.Utils;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.Installer.Bootstrapper.ViewModels;

public class InstallPreparationViewModel : BindableBase
{
    private readonly PowerLabBootstrapper _ba;
    private readonly IRegionManager _regionManager;
    private readonly IRegionDialogService _regionDialogService;
    private bool _agreeAgreement;
    private bool _isCreateDesktopFolderShortcut;
    private bool _isRunAtBoot;
    #region Command
    /// <summary>
    /// 打开协议声明
    /// </summary>
    public DelegateCommand OpenCloudServiceAgreementCommand { get; }
    private void OpenCloudServiceAgreement()
    {
        string currentFolderPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        string cloudServiceAgreementFilePath = Path.Combine(currentFolderPath, "协议声明.pdf");
        Task.Run(() => Process.Start(cloudServiceAgreementFilePath));
    }

    /// <summary>
    /// 执行安装命令
    /// </summary>
    public DelegateCommand InstallCommand { get; }
    private bool CanInstall() => AgreeAgreement;
    private async void Install()
    {
        if (!await StopAppAsync()) return;

        _regionManager.RequestNavigate(
            InstallerRegionNames.MainRegion,
            InstallerViewNames.Installing,
            new NavigationParameters
            {
                { "IsCreateDesktopFolderShortcut", IsCreateDesktopFolderShortcut },
                { "IsRunAtBoot", IsRunAtBoot }
            });
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

        object value = key.GetValue("InstallPath");
        return value?.ToString();
    }
    #endregion

    #region 属性
    /// <summary>
    /// 是否创建桌面快捷方式
    /// </summary>
    public bool IsCreateDesktopFolderShortcut
    {
        get => _isCreateDesktopFolderShortcut;
        set => SetProperty(ref _isCreateDesktopFolderShortcut, value);
    }
    /// <summary>
    /// 开机自启
    /// </summary>
    public bool IsRunAtBoot
    {
        get => _isRunAtBoot;
        set => SetProperty(ref _isRunAtBoot, value);
    }
    /// <summary>
    /// 同意协议
    /// </summary>
    public bool AgreeAgreement
    {
        get => _agreeAgreement;
        set => SetProperty(ref _agreeAgreement, value);
    }

    public Version Version => _ba.Version;

    public string TargetInstallFolder
    {
        get => _ba.InstallDirectory;
        set
        {
            string targetFolder = value;
            if (Directory.GetFiles(targetFolder).Length != 0 || Directory.GetDirectories(targetFolder).Length != 0)
            {
                targetFolder = Path.Combine(targetFolder, "PowerLab");
            }
            _ba.InstallDirectory = targetFolder;
        }
    }
    #endregion 

    #region 构造函数
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="model"></param>
    public InstallPreparationViewModel(
        PowerLabBootstrapper ba,
        IRegionManager regionManager,
        IRegionDialogService regionDialogService)
    {
        _ba = ba;
        _regionManager = regionManager; 
        _regionDialogService = regionDialogService;

        IsCreateDesktopFolderShortcut = true;
        IsRunAtBoot = true;

        InstallCommand = new DelegateCommand(Install, CanInstall).ObservesProperty(() => AgreeAgreement);
        OpenCloudServiceAgreementCommand = new DelegateCommand(OpenCloudServiceAgreement);
    }
    #endregion
}
