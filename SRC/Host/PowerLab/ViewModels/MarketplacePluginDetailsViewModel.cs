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
        private IRegionManager _regionManager;

        public AppPlugin Plugin
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public DelegateCommand BackToHomeCommand { get; }
        private void BackToHome()
        {
            _regionManager.RequestNavigate(
                HostRegionNames.MainRegion,
                ViewNames.PluginsMarketplace);
        }
        public MarketplacePluginDetailsViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            BackToHomeCommand = new DelegateCommand(BackToHome);
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
