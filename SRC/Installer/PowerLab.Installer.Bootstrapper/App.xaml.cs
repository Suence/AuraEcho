using System.Configuration;
using System.Data;
using System.Windows;
using PowerLab.Installer.Bootstrapper.Views;
using Prism.DryIoc;
using Prism.Ioc;

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
        }
    }

}
