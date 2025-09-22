using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.Extensions;
using PowerLab.Installer.Bootstrapper.WixToolset;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private readonly PowerLabBootstrapper _ba;

    private void DetectCompleted(object? sender, EventArgs e)
    {
        var targetView = _ba.Downgrade switch
        {
            true => InstallerViewNames.DowngradeDetected,
            false when _ba.Command.Action == LaunchAction.Uninstall => InstallerViewNames.UninstallPreparation,
            _ => InstallerViewNames.InstallPreparation
        };

        _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, targetView);
    }

    public DelegateCommand ExitCommand { get; }
    private void Exit()
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="model"></param>
    public MainWindowViewModel(PowerLabBootstrapper ba, IRegionManager regionManager)
    {
        _ba = ba;
        _regionManager = regionManager;
        ExitCommand = new DelegateCommand(Exit);

        _ba.OnActionRequested += DetectCompleted;
        _ba.Engine.Detect();
    }
}
