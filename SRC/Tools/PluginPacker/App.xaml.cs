using System.Configuration;
using System.Data;
using System.Windows;
using PluginPacker.Constants;
using PluginPacker.Views;
using PowerLab.Core.Contracts;
using PowerLab.Core.Services;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;

namespace PluginPacker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Homepage>();
            containerRegistry.RegisterSingleton<ILogger, SerilogService>();
            containerRegistry.RegisterSingleton<IFileDialogService, FileDialogService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.MainRegion, ViewNames.Homepage);
        }
    }

}
