using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using PowerLab.Core.Attributes;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
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
        private readonly IRegionManager _regionManager;
        private readonly ILogger _logger;
        private readonly IModuleManager _moduleManager;
        private readonly IModuleCatalog _moduleCatalog;

        private ObservableCollection<PluginMetadata> _plugins;
        #endregion

        public ObservableCollection<PluginMetadata> Plugins
        {
            get => _plugins ??= new ObservableCollection<PluginMetadata>();
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

        public DelegateCommand LoadPluginsCommand { get; }
        private void LoadPlugins()
        {
            string[] pluginFolders = Directory.GetDirectories(ApplicationPaths.Plugins);
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
                string entryAssemblyPath = Path.Combine(pluginFolder, manifest.EntryAssemblyName);
                Assembly pluginAssembly = Assembly.LoadFile(entryAssemblyPath);

                LoadPlugin(pluginAssembly);

                PluginDefaultViewAttribute defaultView = pluginAssembly.GetCustomAttributes<PluginDefaultViewAttribute>().FirstOrDefault();
                if (defaultView is null)
                {
                    _logger.Error($"插件 {manifest.PluginName} 没有指定默认视图。");
                    continue;
                }

                Plugins.Add(new PluginMetadata
                {
                    Id = manifest.Id,
                    Name = manifest.PluginName,
                    DefaultView = defaultView.ViewName,
                });
            }
        }

        public DelegateCommand<PluginMetadata> SwitchPluginCommand { get; }
        private void SwitchPlugin(PluginMetadata pluginMetadata)
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

            LoadPluginsCommand = new DelegateCommand(LoadPlugins);
            SwitchPluginCommand = new DelegateCommand<PluginMetadata>(SwitchPlugin);
        }

        private void LoadPlugin(Assembly pluginAssembly)
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
