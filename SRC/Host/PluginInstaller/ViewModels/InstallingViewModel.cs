using System.IO;
using PluginInstaller.Constants;
using PluginInstaller.Tools;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Host.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PluginInstaller.ViewModels
{
    public class InstallingViewModel : BindableBase, INavigationAware
    {
        #region private members
        private readonly IRegionManager _regionManager;
        private readonly ILogger _logger;

        private string _pluginTempDir;
        private PluginManifest _pluginManifest;
        #endregion

        public PluginManifest PluginManifest
        {
            get => _pluginManifest;
            set => SetProperty(ref _pluginManifest, value);
        }

        public DelegateCommand InstallPluginCommand { get; }
        private void InstallPlugin()
        {
            // 拷贝到目标插件目录
            string finalPath = Path.Combine(ApplicationPaths.Plugins, PluginManifest.Id);
            if (Directory.Exists(finalPath))
                Directory.Delete(finalPath, true);

            DirectoryUtils.SafeMoveDirectory(_pluginTempDir, finalPath);

            _regionManager.RequestNavigate(
                RegionNames.MainRegion,
                ViewNames.InstallCompleted);
        }

        public InstallingViewModel(IRegionManager regionManager, ILogger logger)
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InstallPluginCommand = new DelegateCommand(InstallPlugin);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _pluginTempDir = navigationContext.Parameters.GetValue<string>("PluginTempDir");
            PluginManifest = navigationContext.Parameters.GetValue<PluginManifest>("PluginManifest");
        }
    }
}
