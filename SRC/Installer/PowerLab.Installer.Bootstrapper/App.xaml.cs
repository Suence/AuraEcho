using System.Diagnostics;
using System.Windows;
using PowerLab.Installer.Bootstrapper.Views;
using Prism.Ioc;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<PowerLabBootstrapper>(() => PowerLabBootstrapper.Instance);

            containerRegistry.RegisterForNavigation<InstallPreparation>();
            containerRegistry.RegisterForNavigation<Installing>();
            containerRegistry.RegisterForNavigation<InstallFinish>();
            containerRegistry.RegisterForNavigation<UninstallPreparation>();
            containerRegistry.RegisterForNavigation<Uninstalling>();
            containerRegistry.RegisterForNavigation<UninstallFinish>();
        }

        static void Main()
        {
            Debugger.Launch();
            ManagedBootstrapperApplication.Run(PowerLabBootstrapper.Instance);
        }
    }
}
