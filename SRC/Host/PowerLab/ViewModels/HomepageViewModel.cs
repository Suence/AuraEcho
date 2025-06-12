using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using PowerLab.Constants;
using PowerLab.Core.Attributes;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Extensions;
using PowerLab.Host.Core.Models;
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

        public ObservableCollection<PluginRegistry> Plugins
        {
            get => _plugins ??= new ObservableCollection<PluginRegistry>();
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
        private void NavigationToDashboard()
        {
            _regionManager.RequestNavigate(HostRegionNames.PluginContentRegion, ViewNames.PluginsDashboard, new NavigationParameters
            {
                { "PluginRegistries", Plugins }
            });
        }


        public DelegateCommand LoadPluginsCommand { get; }
        private void LoadPlugins()
        {
            List<(PluginManifest Manifest, string PluginFolder)> pluginManifests = LoadPluginManifest();
            List<PluginRegistry> pluginRegistries = LoadPluginRegistry();

            foreach (var (manifest, pluginFolder) in pluginManifests)
            {
                var pluginRegistry = pluginRegistries.FirstOrDefault(pr => pr.Id == manifest.Id);

                // 如果插件被禁用，跳过加载
                if (pluginRegistry != null && !pluginRegistry.IsEnabled)
                {
                    _logger.Debug($"插件 {pluginRegistry.Name} 已被禁用，跳过加载。");
                    continue;
                }

                // 统一加载插件程序集和读取 DefaultView
                string entryAssemblyPath = Path.Combine(pluginFolder, manifest.EntryAssemblyName);

                if (!File.Exists(entryAssemblyPath))
                {
                    _logger.Error($"插件 {manifest.PluginName} 主程序集不存在：{entryAssemblyPath}");
                    continue;
                }

                Assembly pluginAssembly;
                try
                {
                    pluginAssembly = Assembly.LoadFile(entryAssemblyPath);
                }
                catch (Exception ex)
                {
                    _logger.Error($"加载插件程序集失败：{manifest.PluginName}，异常：{ex.Message}");
                    continue;
                }

                var defaultView = pluginAssembly.GetCustomAttributes<PluginDefaultViewAttribute>().FirstOrDefault();
                if (defaultView is null)
                {
                    _logger.Error($"插件 {manifest.PluginName} 没有指定默认视图。");
                    continue;
                }

                if (pluginRegistry == null)
                {
                    // 第一次加载新插件，添加到注册列表并默认启用
                    pluginRegistry = new PluginRegistry
                    {
                        Id = manifest.Id,
                        Name = manifest.PluginName,
                        DefaultView = defaultView.ViewName,
                        IsEnabled = true,
                        PluginFolder = pluginFolder
                    };
                    pluginRegistries.Add(pluginRegistry);
                }
                else
                {
                    // 更新默认视图（防止之前版本旧）
                    pluginRegistry.DefaultView = defaultView.ViewName;
                }

                LoadPluginEntryAssembly(pluginAssembly);
            }

            Plugins = pluginRegistries.ToObservableCollection();

            _logger.Debug($"已加载 {Plugins.Count} 个插件。");
            string pluginRegistriesJson = JsonSerializer.Serialize(pluginRegistries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_pluginRegistryPath, pluginRegistriesJson);

            NavigationToDashboard();
        }

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
        private void SwitchPlugin(PluginRegistry pluginMetadata)
        {
            if (pluginMetadata is null)
                return;

            _regionManager.RequestNavigate(
                HostRegionNames.PluginContentRegion,
                pluginMetadata.DefaultView);
        }

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

        private void LoadPluginEntryAssembly(Assembly pluginAssembly)
        {
            Assembly moduleAssembly = AppDomain.CurrentDomain.GetAssemblies().First(asm => asm.FullName == typeof(IModule).Assembly.FullName);
            Type IModuleType = moduleAssembly.GetType(typeof(IModule).FullName);

            var moduleInfos = pluginAssembly.GetExportedTypes()
                .Where(IModuleType.IsAssignableFrom)
                .Where(t => t != IModuleType)
                .Where(t => !t.IsAbstract)
                .Select(CreateModuleInfo);

            foreach (var moduleInfo in moduleInfos)
            {
                _moduleCatalog.AddModule(moduleInfo);

                var d = Application.Current.Dispatcher;
                if (d.CheckAccess())
                    _moduleManager.LoadModule(moduleInfo.ModuleName);
                else
                    d.BeginInvoke(() => _moduleManager.LoadModule(moduleInfo.ModuleName));
            }
        }

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
