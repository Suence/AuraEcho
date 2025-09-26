using Microsoft.Win32;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.Extensions;
using PowerLab.Installer.Bootstrapper.WixToolset;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Diagnostics;
using System.IO;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper.ViewModels;

public class InstallingViewModel : BindableBase, INavigationAware
{
    private readonly PowerLabBootstrapper _ba;
    private readonly IRegionManager _regionManager;
    private readonly IRegionDialogService _regionDialogService;

    private string message;
    private int _progress;
    private bool _isCreateDesktopFolderShortcut;
    private bool _isRunAtBoot;
    #region Command
    /// <summary>
    /// 执行安装命令
    /// </summary>
    public DelegateCommand InstallCommand { get; }
    private async void Install()
    {
        await StopAppAsync();

        _ba.Install();
    }

    private async Task StopAppAsync()
    {
        List<Process> allProcesses =
            [.. Process.GetProcessesByName("PowerLab"),
             .. Process.GetProcessesByName("PlixInstaller")];

        if (allProcesses.Count <= 0) return;

        string? installFolder = Path.GetDirectoryName(GetInstallPath());
        List<Process> runningProcesses =
            [.. allProcesses.Where(p => Path.GetDirectoryName(p.MainModule.FileName) == installFolder)];

        if (runningProcesses.Count <= 0) return;

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
        if (dialogResult != RegionDialogResult.OK)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            await StopAppAsync();
            return;
        }

        runningProcesses.ForEach(p => p.Kill());
    }

    private string GetInstallPath()
    {
        const string keyPath = @"Software\Suencesoft\PowerLab";
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath);
        if (key == null) return null;

        object value = key.GetValue("InstallPath");
        return value?.ToString();
    }

    public DelegateCommand CancelCommand { get; }
    private void Cancel()
    {
        _ba.Cancel();
    }

    #endregion

    #region 属性

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }
    /// <summary>
    /// 安装总进度
    /// </summary>
    public int Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }


    #endregion 

    #region 构造函数
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="model"></param>
    public InstallingViewModel(PowerLabBootstrapper ba, IRegionManager regionManager, IRegionDialogService regionDialogService)
    {
        _ba = ba;
        _regionManager = regionManager;
        _regionDialogService = regionDialogService;

        InstallCommand = new DelegateCommand(Install);
        CancelCommand = new DelegateCommand(Cancel);
        SubscriptionInstallEvents();
    }

    #endregion

    #region 方法

    private void SubscriptionInstallEvents()
    {
        _ba.OnActionCompleted += InstallCompleted;
        _ba.OnPlanMsiFeature += PlanMsiFeature;
        _ba.OnExecuteMsiMessage += ExecuteMsiMessage;
        _ba.OnProgress += UpdateProgress;
    }

    private void ExecuteMsiMessage(object? sender, string e)
    {
        Message = e;
    }


    private void UpdateProgress(object? sender, int e)
    {
        Progress = e;
    }

    private void InstallCompleted(object? sender, EventArgs e)
    {
        if (_ba.CancelRequested)
        {
            _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, InstallerViewNames.ActionCancelled);
            return;
        }

        Message = "正在完成安装...";
        DataMigration();

        _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, InstallerViewNames.InstallFinish);
    }

    private void DataMigration()
    {
        var installFolder = Path.GetDirectoryName(GetInstallPath());
        var dataMigratorPath = Path.Combine(installFolder, "PowerLab.DataMigrator.exe");

        var dataMigratorStartupInfo = new ProcessStartInfo
        {
            FileName = dataMigratorPath,
            CreateNoWindow = true
        };
        var dataMigratorProcess = Process.Start(dataMigratorStartupInfo);

        dataMigratorProcess.WaitForExit();
    }

    private void UnsubscriptionInstallEvents()
    {
        _ba.OnActionCompleted -= InstallCompleted;
        _ba.OnPlanMsiFeature -= PlanMsiFeature;
        _ba.OnExecuteMsiMessage -= ExecuteMsiMessage;
        _ba.OnProgress -= UpdateProgress;
    }

    private void PlanMsiFeature(object sender, PlanMsiFeatureEventArgs e)
    {
        if (e.FeatureId == "DesktopShortcut")
        {
            e.State = _isCreateDesktopFolderShortcut ? FeatureState.Local : FeatureState.Absent;
            return;
        }
        if (e.FeatureId == "RunAtBoot")
        {
            e.State = _isRunAtBoot ? FeatureState.Local : FeatureState.Absent;
            return;
        }
        e.State = FeatureState.Local;
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _isCreateDesktopFolderShortcut = (bool)navigationContext.Parameters["IsCreateDesktopFolderShortcut"];
        _isRunAtBoot = (bool)navigationContext.Parameters["IsRunAtBoot"];
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
        => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        UnsubscriptionInstallEvents();
    }

    #endregion
}
