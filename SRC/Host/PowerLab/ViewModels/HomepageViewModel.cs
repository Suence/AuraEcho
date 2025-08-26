using System.Collections.ObjectModel;
using System.Linq;
using PowerLab.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Extensions;
using PowerLab.Core.Models;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class HomepageViewModel : BindableBase
    {
        private string _title = "PowerLab";
        private readonly IRegionManager _regionManager;
        private readonly IThemeManager _themeManager;
        private readonly ILogger _logger;
        private ObservableCollection<PluginRegistry> _plugins;

        private readonly IPluginManager _pluginManager;

        public ObservableCollection<PluginRegistry> Plugins
        {
            get => _plugins ??= [];
            set => SetProperty(ref _plugins, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DelegateCommand NavigationToDashboardCommand { get; }
        public DelegateCommand NavigationToSettingsCommand { get; }
        public DelegateCommand LoadPluginsCommand { get; }
        public DelegateCommand<PluginRegistry> SwitchPluginCommand { get; }

        private void NavigationToDashboard()
        {
            _regionManager.RequestNavigate(HostRegionNames.HomeContentRegion, ViewNames.PluginsDashboard, new NavigationParameters
            {
                { "PluginRegistries", Plugins }
            });
        }

        private void NavigationToSettings()
        {
            _regionManager.RequestNavigate(HostRegionNames.DialogRegion, ViewNames.Settings);
        }

        private async void LoadPlugins()
        {
            var pluginRegistries = await _pluginManager.LoadPluginsAsync();
            Plugins = pluginRegistries.ToObservableCollection();
            
            _themeManager.AttachPluginThemes(
                pluginRegistries.Select(p => p.PluginContext)
                                .Where(p => p is not null));

            NavigationToDashboard();
        }

        private void SwitchPlugin(PluginRegistry pluginMetadata)
        {
            if (pluginMetadata is null)
                return;

            _regionManager.RequestNavigate(
                HostRegionNames.HomeContentRegion,
                pluginMetadata.DefaultView);
        }

        public HomepageViewModel(IRegionManager regionManager, IPluginManager pluginManager, IThemeManager themeManager, ILogger logger)
        {
            _regionManager = regionManager;
            _themeManager = themeManager;
            _logger = logger;
            _pluginManager = pluginManager;

            LoadPluginsCommand = new DelegateCommand(LoadPlugins);
            SwitchPluginCommand = new DelegateCommand<PluginRegistry>(SwitchPlugin);
            NavigationToSettingsCommand = new DelegateCommand(NavigationToSettings);
            NavigationToDashboardCommand = new DelegateCommand(NavigationToDashboard);
        }
    }

}
