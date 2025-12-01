using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PowerLab.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.PluginContracts.Constants;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class PluginsMarketplaceViewModel : BindableBase, IRegionMemberLifetime
    {
        private readonly IPluginRespository _pluginRespository;
        private readonly IRegionManager _regionManager;

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

            Plugins = [.. result];
        }

        public DelegateCommand<AppPlugin> NavigationToPluginDetailsCommand { get; }
        private void NavigationToPluginDetails(AppPlugin plugin)
        {
            _regionManager.RequestNavigate(
                HostRegionNames.MainRegion,
                ViewNames.MarketplacePluginDetails,
                new NavigationParameters
                {
                    { "Plugin", plugin }
                });
        }

        public PluginsMarketplaceViewModel(IPluginRespository pluginRespository, IEventAggregator eventAggregator, IRegionManager regionManager, IFileRespository fileRespository)
        {
            _pluginRespository = pluginRespository;
            _regionManager = regionManager;

            LoadPluginsCommand = new DelegateCommand(LoadPlugins);
            NavigationToPluginDetailsCommand = new DelegateCommand<AppPlugin>(NavigationToPluginDetails);
            LoadPlugins();
        }
        public bool KeepAlive => false;
    }
}
