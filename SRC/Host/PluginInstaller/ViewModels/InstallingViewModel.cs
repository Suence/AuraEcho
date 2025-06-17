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
    /// <summary>
    /// 正在安装
    /// </summary>
    public class InstallingViewModel : BindableBase, INavigationAware
    {
        #region private members
        private readonly IRegionManager _regionManager;
        private readonly ILogger _logger;

        private string _pluginTempDir;
        private PluginManifest _pluginManifest;
        #endregion

        /// <summary>
        /// 模块清单
        /// </summary>
        public PluginManifest PluginManifest
        {
            get => _pluginManifest;
            set => SetProperty(ref _pluginManifest, value);
        }

        public DelegateCommand InstallPluginCommand { get; }
        /// <summary>
        /// 安装模块
        /// </summary>
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

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
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
