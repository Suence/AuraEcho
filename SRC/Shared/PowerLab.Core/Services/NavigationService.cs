using PowerLab.Core.Models;
using PowerLab.PluginContracts.Interfaces;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.Core.Services
{
    public class NavigationService(IRegionManager regionManager) : BindableBase, INavigationService
    {
        private readonly IRegionManager _regionManager = regionManager;
        private readonly Stack<NavigationHistoryEntry> _stack = new();

        public void RequestNavigate(string regionName, string target, NavigationParameters? navigationParameters = null)
        {
            var entry = new NavigationHistoryEntry(regionName, target, navigationParameters);
            if (_stack.FirstOrDefault() != entry)
                _stack.Push(entry);

            _regionManager.RequestNavigate(regionName, target, navigationParameters);
            RaisePropertyChanged(nameof(CanGoBack));
        }

        public bool CanGoBack => _stack.Count > 0;

        public void GoBack()
        {
            if (_stack.Count == 0)
                return;

            if (_stack.Count == 1)
            {
                _regionManager.Regions[_stack.Pop().RegionName].RemoveAll();
                RaisePropertyChanged(nameof(CanGoBack));
                return;
            }

            var topEntry = _stack.Pop();
            var entry = _stack.Peek();

            if (topEntry.RegionName != entry.RegionName)
            {
                var region = _regionManager.Regions[topEntry.RegionName];
                region.RemoveAll();
            }

            _regionManager.RequestNavigate(entry.RegionName, entry.ViewName, entry.Parameters);
            RaisePropertyChanged(nameof(CanGoBack));
        }
    }

}
