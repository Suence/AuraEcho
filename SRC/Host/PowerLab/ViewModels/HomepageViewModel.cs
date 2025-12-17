using System.Collections.ObjectModel;
using System.Linq;
using PowerLab.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Events;
using PowerLab.Core.Extensions;
using PowerLab.Core.Models;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels;

public class HomepageViewModel : BindableBase
{
    private string _title = "PowerLab";
    private readonly IRegionManager _regionManager;
    private readonly ILocalPluginRepository _localPluginRepository;
    private readonly INavigationService _navigationService;
    private readonly IEventAggregator _eventAggregator;
    private readonly IThemeManager _themeManager;
    private readonly ILogger _logger;
    private ObservableCollection<PluginRegistryModel> _plugins;

    private readonly IPluginManager _pluginManager;

    public ObservableCollection<PluginRegistryModel> Plugins
    {
        get => _plugins ??= [];
        set => SetProperty(ref _plugins, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public DelegateCommand NavigationToPluginsMarketplaceCommand { get; }
    private void NavigationToPluginsMarketplace()
    {
        _navigationService.RequestNavigate(HostRegionNames.MainRegion, ViewNames.PluginsMarketplace);
    }

    public DelegateCommand NavigationToSettingsCommand { get; }
    private void NavigationToSettings()
    {
        _navigationService.RequestNavigate(HostRegionNames.MainRegion, ViewNames.Settings);
    }

    public DelegateCommand LoadPluginsCommand { get; }
    private async void LoadPlugins()
    {
        var pluginRegistries = await _pluginManager.LoadPluginsAsync();
        Plugins = pluginRegistries.ToObservableCollection();

        _themeManager.AttachPluginThemes(
            pluginRegistries.Select(p => p.PluginContext)
                            .Where(p => p is not null));
    }

    public DelegateCommand<PluginRegistryModel> PluginPlanUninstallCommand { get; }
    private void PluginPlanUninstall(PluginRegistryModel plugin)
    {
        plugin.PlanStatus = PluginPlanStatus.UninstallPending;
        _localPluginRepository.UpdatePluginRegistry(plugin);
    }

    public DelegateCommand<PluginRegistryModel> CancelPluginPlanUninstallCommand { get; }
    private void CancelPluginPlanUninstall(PluginRegistryModel plugin)
    {
        if (plugin.PlanStatus != PluginPlanStatus.UninstallPending) return;

        plugin.PlanStatus = PluginPlanStatus.None;
        _localPluginRepository.UpdatePluginRegistry(plugin);
    }

    public DelegateCommand<PluginRegistryModel> SwitchPluginCommand { get; }
    private void SwitchPlugin(PluginRegistryModel pluginMetadata)
    {
        if (pluginMetadata is null)
            return;

        _navigationService.RequestNavigate(
            HostRegionNames.MainRegion,
            pluginMetadata.Manifest.DefaultViewName);
    }

    public HomepageViewModel(
        INavigationService navigationService, 
        IRegionManager regionManager, 
        ILocalPluginRepository localPluginRepository,
        IEventAggregator eventAggregator,
        IPluginManager pluginManager, 
        IThemeManager themeManager, 
        ILogger logger)
    {
        _regionManager = regionManager;
        _navigationService = navigationService;
        _localPluginRepository = localPluginRepository;
        _eventAggregator = eventAggregator;
        _themeManager = themeManager;
        _logger = logger;
        _pluginManager = pluginManager;

        LoadPluginsCommand = new DelegateCommand(LoadPlugins);
        SwitchPluginCommand = new DelegateCommand<PluginRegistryModel>(SwitchPlugin);
        NavigationToSettingsCommand = new DelegateCommand(NavigationToSettings);
        NavigationToPluginsMarketplaceCommand = new DelegateCommand(NavigationToPluginsMarketplace);
        PluginPlanUninstallCommand = new DelegateCommand<PluginRegistryModel>(PluginPlanUninstall);
        CancelPluginPlanUninstallCommand = new DelegateCommand<PluginRegistryModel>(CancelPluginPlanUninstall);

        _eventAggregator.GetEvent<PluginInstalledEvent>().Subscribe(AddNewPlugin);

        LoadPlugins();
    }

    private void AddNewPlugin(PluginRegistryModel registry)
    {
        Plugins.Add(registry);
    }
}
