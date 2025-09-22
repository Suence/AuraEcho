using PowerLab.Installer.Bootstrapper.Views;
using PowerLab.Installer.Bootstrapper.WixToolset;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.UIToolkit.RegionDialog;
using Prism.Ioc;
using System.Diagnostics;
using System.Windows;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly Mutex _mutex = new(false, "17FA29D6-F4BC-4720-A55C-27042D247E35");
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<PowerLabBootstrapper>(() => PowerLabBootstrapper.Instance);

        containerRegistry.Register<IRegionDialogService, RegionDialogService>();
        containerRegistry.RegisterForNavigation<ConfirmDialog>();

        containerRegistry.RegisterForNavigation<InstallPreparation>();
        containerRegistry.RegisterForNavigation<Installing>();
        containerRegistry.RegisterForNavigation<InstallFinish>();
        containerRegistry.RegisterForNavigation<UninstallPreparation>();
        containerRegistry.RegisterForNavigation<Uninstalling>();
        containerRegistry.RegisterForNavigation<UninstallFinish>();
        containerRegistry.RegisterForNavigation<ActionCancelled>();
        containerRegistry.RegisterForNavigation<DowngradeDetected>();
    }

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        try
        {
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                new InstallerAlreadyRunningDialog().ShowDialog();
                Shutdown();
                return;
            }
        }
        catch(AbandonedMutexException) { }

        base.OnStartup(e);
    }

    static void Main()
    {
        Debugger.Launch();
        ManagedBootstrapperApplication.Run(PowerLabBootstrapper.Instance);
    }
}
