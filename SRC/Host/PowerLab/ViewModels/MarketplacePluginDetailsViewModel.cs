using PowerLab.Constants;
using PowerLab.Core.Models;
using PowerLab.PluginContracts.Constants;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class MarketplacePluginDetailsViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        public AppPlugin Plugin
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public MarketplacePluginDetailsViewModel()
        {
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
        }
    }
}
