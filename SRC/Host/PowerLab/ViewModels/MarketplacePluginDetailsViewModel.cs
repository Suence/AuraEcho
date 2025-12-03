using System;
using System.Threading.Tasks;
using PowerLab.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.PluginContracts.Constants;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class MarketplacePluginDetailsViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private readonly IPluginRespository _pluginRespository;

        public AppPlugin Plugin
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public PluginPackage LatestVersionInfo
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        private async Task LoadPluginDetails()
        {
            var result = await _pluginRespository.GetLatestAsync(Plugin.Id);
            if (result is null) return;

            LatestVersionInfo = result;
        }
        public MarketplacePluginDetailsViewModel(IPluginRespository pluginRespository)
        {
            _pluginRespository = pluginRespository;
        }

        public bool KeepAlive => false;

        public bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Plugin = navigationContext.Parameters["Plugin"] as AppPlugin;
            _ = LoadPluginDetails();
        }
    }
}
