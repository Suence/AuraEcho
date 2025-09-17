using System.IO;
using System.Reflection;
using PluginInstaller.Constants;
using PluginInstaller.Tools;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;
using PowerLab.PluginContracts.Attributes;
using PowerLab.PluginContracts.Interfaces;
using Prism.Commands;
using Prism.Ioc;
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
        private readonly IPluginRepository _pluginRepository;
        private readonly ILogger _logger;
        private readonly IContainerProvider _containerProvider;

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

            var existingPlugin = _pluginRepository.GetPluginRegistries().FirstOrDefault(pr => pr.Manifest.Id == PluginManifest.Id);
            if (existingPlugin is not null)
            {
                _pluginRepository.RemovePluginRegistry(existingPlugin.Id);
            }

            var entryAssemblyPath = Path.Combine(finalPath, PluginManifest.EntryAssemblyName);
            var alc = new PluginLoadContext(entryAssemblyPath);
            Assembly pluginAssembly = null;
            try
            {
                pluginAssembly = alc.LoadFromAssemblyPath(entryAssemblyPath);
            }
            catch (Exception ex)
            {
                _logger.Error($"加载插件程序集失败：{PluginManifest.PluginName}，异常：{ex.Message}");
            }
            string defaultView = GetPluginDefaultView(pluginAssembly);

            IPluginSetup pluginDatabaseInitializer = GetPluginDatabaseInitializer(pluginAssembly);
            if (pluginDatabaseInitializer is not null)
            {
                pluginDatabaseInitializer.Setup(_containerProvider);
            }
            _pluginRepository.AddPluginRegistry(new PluginRegistry
            {
                Id = Guid.NewGuid().ToString(),
                PlanStatus = PluginPlanStatus.None,
                Manifest = PluginManifest,
                PluginFolder = ApplicationPaths.GetPluginPath(PluginManifest.Id),
                Status = PluginStatus.Enabled,
                DefaultView = defaultView
            });

            _regionManager.RequestNavigate(
                RegionNames.MainRegion,
                ViewNames.InstallCompleted);
        }

        private IPluginSetup GetPluginDatabaseInitializer(Assembly pluginAssembly)
        {
            Type? pluginDatabaseInitializerType =
                pluginAssembly.GetExportedTypes()
                              .Where(t => typeof(IPluginSetup).IsAssignableFrom(t))
                              .Where(t => t != typeof(IPluginSetup))
                              .Where(t => !t.IsAbstract)
                              .SingleOrDefault();
            return _containerProvider.Resolve(pluginDatabaseInitializerType) as IPluginSetup;
        }

        private string GetPluginDefaultView(Assembly pluginAssembly)
        {
            var targetAttribute = pluginAssembly.GetCustomAttributes<PluginDefaultViewAttribute>().FirstOrDefault();
            if (targetAttribute is null) throw new Exception("插件程序集没有指定 DefaultView");

            return targetAttribute.ViewName;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public InstallingViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IPluginRepository pluginRepository, ILogger logger)
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _pluginRepository = pluginRepository ?? throw new ArgumentNullException(nameof(pluginRepository));
            _containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));

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
