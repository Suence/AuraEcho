using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using PowerLab.Constants;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Extensions;
using PowerLab.Core.Models;
using PowerLab.PluginContracts.Attributes;
using PowerLab.PluginContracts.Constants;
using PowerLab.Tools;
using Prism.Commands;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class HomepageViewModel : BindableBase
    {
        #region private members
        private string _title = "PowerLab";
        private string _pluginRegistryPath;
        private readonly IRegionManager _regionManager;
        private readonly ILogger _logger;
        private readonly IModuleManager _moduleManager;
        private readonly IModuleCatalog _moduleCatalog;
        private ObservableCollection<PluginRegistry> _plugins;
        #endregion

        /// <summary>
        /// 扩展模块列表
        /// </summary>
        public ObservableCollection<PluginRegistry> Plugins
        {
            get => _plugins ??= [];
            set => SetProperty(ref _plugins, value);
        }

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public DelegateCommand NavigationToDashboardCommand { get; }
        /// <summary>
        /// 导航到仪表盘
        /// </summary>
        private void NavigationToDashboard()
        {
            _regionManager.RequestNavigate(HostRegionNames.PluginContentRegion, ViewNames.PluginsDashboard, new NavigationParameters
            {
                { "PluginRegistries", Plugins }
            });
        }

        public DelegateCommand LoadPluginsCommand { get; }
        /// <summary>
        /// 加载扩展模块
        /// </summary>
        private void LoadPlugins()
        {
            List<(PluginManifest Manifest, string PluginFolder)> pluginManifests = LoadPluginManifest();
            List<PluginRegistry> pluginRegistries = LoadPluginRegistry();

            foreach (var (manifest, pluginFolder) in pluginManifests)
            {
                var pluginRegistry = pluginRegistries.FirstOrDefault(pr => pr.Manifest.Id == manifest.Id);

                if (pluginRegistry != null)
                {
                    if (pluginRegistry.PlanStatus != PluginPlanStatus.None)
                    {
                        // 卸载处理
                        if (pluginRegistry.PlanStatus == PluginPlanStatus.UninstallPending)
                        {
                            Directory.Delete(pluginRegistry.PluginFolder, true);
                            pluginRegistries.Remove(pluginRegistry);
                            _logger.Debug($"插件 {pluginRegistry.Manifest.PluginName} 已被卸载，跳过加载。");
                            continue;
                        }

                        pluginRegistry.Status = pluginRegistry.PlanStatus switch
                        {
                            PluginPlanStatus.EnablePending => PluginStatus.Enabled,
                            PluginPlanStatus.DisablePending => PluginStatus.Disabled,
                            _ => pluginRegistry.Status
                        };
                        pluginRegistry.PlanStatus = PluginPlanStatus.None;
                    }

                    // 已禁用
                    if (pluginRegistry.Status == PluginStatus.Disabled)
                    {
                        _logger.Debug($"插件 {pluginRegistry.Manifest.PluginName} 已被禁用，跳过加载。");
                        continue;
                    }
                }


                string entryAssemblyPath = Path.Combine(pluginFolder, manifest.EntryAssemblyName);

                if (!File.Exists(entryAssemblyPath))
                {
                    _logger.Error($"插件 {manifest.PluginName} 主程序集不存在：{entryAssemblyPath}");
                    continue;
                }

                var alc = new PluginLoadContext(entryAssemblyPath);
                Assembly pluginAssembly;
                try
                {
                    pluginAssembly = alc.LoadFromAssemblyPath(entryAssemblyPath);
                }
                catch (Exception ex)
                {
                    _logger.Error($"加载插件程序集失败：{manifest.PluginName}，异常：{ex.Message}");
                    continue;
                }

                PluginDefaultViewAttribute defaultView = pluginAssembly.GetCustomAttributes<PluginDefaultViewAttribute>().FirstOrDefault();
                if (defaultView is null)
                {
                    _logger.Error($"插件 {manifest.PluginName} 没有指定默认视图。");
                    continue;
                }

                if (pluginRegistry == null)
                {
                    pluginRegistry = new PluginRegistry
                    {
                        Manifest = manifest,
                        DefaultView = defaultView.ViewName,
                        Status = PluginStatus.Enabled,
                        PluginFolder = pluginFolder
                    };
                    pluginRegistries.Add(pluginRegistry);
                }

                LoadPluginByAssembly(pluginAssembly);
            }

            Plugins = pluginRegistries.ToObservableCollection();

            _logger.Debug($"已加载 {Plugins.Count} 个插件。");
            string pluginRegistriesJson = JsonSerializer.Serialize(pluginRegistries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_pluginRegistryPath, pluginRegistriesJson);

            NavigationToDashboard();
        }

        /// <summary>
        /// 读取插件配置
        /// </summary>
        /// <returns></returns>
        private List<PluginRegistry> LoadPluginRegistry()
        {
            if (!File.Exists(_pluginRegistryPath))
                return [];

            string registryJson = File.ReadAllText(_pluginRegistryPath);
            var pluginRegistries = JsonSerializer.Deserialize<List<PluginRegistry>>(registryJson);

            if (pluginRegistries == null)
            {
                _logger.Error("无法解析插件注册表。");
                return [];
            }

            return pluginRegistries;
        }

        /// <summary>
        /// 读取插件 manifest 文件
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private List<(PluginManifest Manifest, string PluginFolder)> LoadPluginManifest()
        {
            string[] pluginFolders = Directory.GetDirectories(ApplicationPaths.Plugins);
            List<(PluginManifest, string)> manifests = new();

            foreach (var pluginFolder in pluginFolders)
            {
                string manifestPath = Path.Combine(pluginFolder, "plugin.manifest.json");
                if (!File.Exists(manifestPath))
                    throw new FileNotFoundException("插件缺少 manifest 文件。");

                string manifestJson = File.ReadAllText(manifestPath);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestJson);

                if (manifest == null)
                {
                    _logger.Error($"无法解析插件 {pluginFolder} 的 manifest 文件。");
                    continue;
                }

                manifests.Add((manifest, pluginFolder));
            }

            return manifests;
        }

        public DelegateCommand<PluginRegistry> SwitchPluginCommand { get; }
        /// <summary>
        /// 模块导航
        /// </summary>
        /// <param name="pluginMetadata"></param>
        private void SwitchPlugin(PluginRegistry pluginMetadata)
        {
            if (pluginMetadata is null)
                return;

            _regionManager.RequestNavigate(
                HostRegionNames.PluginContentRegion,
                pluginMetadata.DefaultView);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="moduleManager"></param>
        /// <param name="moduleCatalog"></param>
        /// <param name="logger"></param>
        public HomepageViewModel(IRegionManager regionManager, IModuleManager moduleManager, IModuleCatalog moduleCatalog, ILogger logger)
        {
            _regionManager = regionManager;
            _logger = logger;
            _moduleManager = moduleManager;
            _moduleCatalog = moduleCatalog;

            _pluginRegistryPath = Path.Combine(ApplicationPaths.Data, "plugin.registry.json");
            LoadPluginsCommand = new DelegateCommand(LoadPlugins);
            SwitchPluginCommand = new DelegateCommand<PluginRegistry>(SwitchPlugin);
            NavigationToDashboardCommand = new DelegateCommand(NavigationToDashboard);
        }

        /// <summary>
        /// 加载模块程序集，并注册到 Prism 模块管理器中
        /// </summary>
        /// <param name="pluginAssembly"></param>
        private void LoadPluginByAssembly(Assembly pluginAssembly)
        {
            Type IModuleType = typeof(IModule);

            var moduleTypes = pluginAssembly.GetExportedTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t))
                .Where(t => t != typeof(IModule))
                .Where(t => !t.IsAbstract);

            foreach (var type in moduleTypes)
            {
                ModuleInfo moduleInfo = CreateModuleInfo(type);
                _moduleCatalog.AddModule(moduleInfo);

                var dispatcher = Application.Current.Dispatcher;
                if (dispatcher.CheckAccess())
                    _moduleManager.LoadModule(moduleInfo.ModuleName);
                else
                    dispatcher.BeginInvoke(() => _moduleManager.LoadModule(moduleInfo.ModuleName));
            }
        }

        /// <summary>
        /// 创建 Prism ModuleInfo 对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ModuleInfo CreateModuleInfo(Type type)
        {
            string moduleName = type.Name;

            var moduleAttribute = CustomAttributeData.GetCustomAttributes(type).FirstOrDefault(cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleAttribute).FullName);

            if (moduleAttribute != null)
            {
                foreach (CustomAttributeNamedArgument argument in moduleAttribute.NamedArguments)
                {
                    string argumentName = argument.MemberInfo.Name;
                    if (argumentName == "ModuleName")
                    {
                        moduleName = (string)argument.TypedValue.Value;
                        break;
                    }
                }
            }

            ModuleInfo moduleInfo = new ModuleInfo(moduleName, type.AssemblyQualifiedName)
            {
                InitializationMode = InitializationMode.OnDemand,
                //Ref = type.Assembly.Location,
                Ref = type.Assembly.CodeBase,
            };

            return moduleInfo;
        }
    }
}
