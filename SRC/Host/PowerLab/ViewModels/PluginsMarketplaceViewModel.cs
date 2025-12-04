using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PowerLab.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels;

public class PluginsMarketplaceViewModel : BindableBase, IRegionMemberLifetime
{
    private readonly IRemotePluginRepository _pluginRespository;
    private readonly INavigationService _navigationService;
    private readonly IPluginManager _pluginManager;

    private ObservableCollection<AppPlugin> _plugins;
    public ObservableCollection<AppPlugin> Plugins
    {
        get => _plugins;
        set => SetProperty(ref _plugins, value);
    }

    public DelegateCommand LoadPluginsCommand { get; }
    private async void LoadPlugins()
    {
        var result = await _pluginRespository.GetPluginsAsync();
        if (result is null) return;

        var installedPluginIds = _pluginManager.Plugins.Select(p => p.Manifest.Id).ToList();

        foreach (var plugin in result)
        {
            plugin.IsInstalled = installedPluginIds.Contains(plugin.Id);
        }

        Plugins = [.. result];
    }


    public DelegateCommand<AppPlugin> NavigationToPluginDetailsCommand { get; }
    private void NavigationToPluginDetails(AppPlugin plugin)
    {
        _navigationService.RequestNavigate(
            HostRegionNames.MainRegion,
            ViewNames.MarketplacePluginDetails,
            new NavigationParameters
            {
                { "Plugin", plugin }
            });
    }

    public PluginsMarketplaceViewModel(IPluginManager pluginManager, INavigationService navigationService, IRemotePluginRepository pluginRespository)
    {
        _pluginRespository = pluginRespository;
        _navigationService = navigationService;
        _pluginManager = pluginManager;

        LoadPluginsCommand = new DelegateCommand(LoadPlugins);
        NavigationToPluginDetailsCommand = new DelegateCommand<AppPlugin>(NavigationToPluginDetails);
        LoadPlugins();
    }
    public bool KeepAlive => false;
}
