using PluginPacker.Constants;
using PluginPacker.Views;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Repositories;
using PowerLab.Core.Services;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.UIToolkit.RegionDialog;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Regions;
using System.Windows;

namespace PluginPacker;

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

        containerRegistry.RegisterInstance<IAppLogger>(new Serilogger(ApplicationPaths.Logs));
        containerRegistry.RegisterSingleton<IFileDialogService, FileDialogService>();
        containerRegistry.RegisterSingleton<IRegionDialogService, RegionDialogService>();

        containerRegistry.RegisterSingleton<IFileRepository, FileRepository>();
        containerRegistry.RegisterSingleton<IRemotePluginRepository, RemotePluginRepository>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var regionManager = Container.Resolve<IRegionManager>();
        regionManager.RegisterViewWithRegion(RegionNames.MainRegion, ViewNames.Homepage);
    }
}
