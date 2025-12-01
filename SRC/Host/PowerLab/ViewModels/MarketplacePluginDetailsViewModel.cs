using PowerLab.Core.Models;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class MarketplacePluginDetailsViewModel : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        private IRegionManager _regionManager;

        public AppPlugin Plugin
        {
            get => field;
            set => SetProperty(ref field, value);
        }


        public MarketplacePluginDetailsViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
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
