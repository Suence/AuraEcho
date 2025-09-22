using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Attributes;
using PowerLab.PluginContracts.Interfaces;
using Prism.Modularity;

namespace PowerLab.Services;

public class PluginManager : IPluginManager
{
    private bool _isInitialized;
    private readonly string _pluginRegistryPath;
    private readonly IModuleManager _moduleManager;
    private readonly IModuleCatalog _moduleCatalog;
    private readonly IPluginRepository _pluginRepository;
    private readonly ILogger _logger;
    private readonly List<PluginLoadContext> _pluginLoadContexts = [];

    private List<PluginRegistry> _plugins;
    public List<PluginRegistry> Plugins
    {
        get => _isInitialized ? _plugins : [];
    }

    public PluginManager(IModuleManager moduleManager, IModuleCatalog moduleCatalog, IPluginRepository pluginRepository, ILogger logger)
    {
        _moduleManager = moduleManager;
        _moduleCatalog = moduleCatalog;
        _pluginRepository = pluginRepository;
        _logger = logger;

        _pluginRegistryPath = Path.Combine(ApplicationPaths.Data, "plugin.registry.json");
    }

    /// <summary>
    /// 加载所有插件并返回插件信息
    /// </summary>
    /// <returns></returns>
    public List<PluginRegistry> LoadPlugins()
    {
        // TODO: 线程安全

        if (_isInitialized)
        {
            _logger.Debug("插件管理器已初始化，跳过加载。");
            return _plugins;
        }

        List<PluginRegistry> pluginRegistries = _pluginRepository.GetPluginRegistries().ToList();

        foreach (var pluginRegistry in pluginRegistries.ToList())
        {
            if (pluginRegistry.PlanStatus != PluginPlanStatus.None)
            {
                if (pluginRegistry.PlanStatus == PluginPlanStatus.UninstallPending)
                {
                    if (Directory.Exists(pluginRegistry.PluginFolder))
                    {
                        Directory.Delete(pluginRegistry.PluginFolder, true);
                    }
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

            if (pluginRegistry.Status == PluginStatus.Disabled)
            {
                _logger.Debug($"插件 {pluginRegistry.Manifest.PluginName} 已被禁用，跳过加载。");
                continue;
            }

            string entryAssemblyPath = Path.Combine(ApplicationPaths.GetPluginPath(pluginRegistry.Manifest.Id), pluginRegistry.Manifest.EntryAssemblyName);

            if (!File.Exists(entryAssemblyPath))
            {
                _logger.Error($"插件 {pluginRegistry.Manifest.PluginName} 主程序集不存在：{entryAssemblyPath}");
                continue;
            }

            var alc = new PluginLoadContext(entryAssemblyPath);
            _pluginLoadContexts.Add(alc);
            Assembly pluginAssembly;
            try
            {
                pluginAssembly = alc.LoadFromAssemblyPath(entryAssemblyPath);
            }
            catch (Exception ex)
            {
                _logger.Error($"加载插件程序集失败：{pluginRegistry.Manifest.PluginName}，异常：{ex.Message}");
                continue;
            }

            PluginDefaultViewAttribute defaultView = pluginAssembly.GetCustomAttributes<PluginDefaultViewAttribute>().FirstOrDefault();
            if (defaultView is null)
            {
                _logger.Error($"插件 {pluginRegistry.Manifest.PluginName} 没有指定默认视图。");
                continue;
            }

            IPlugin pluginContext = LoadPluginByAssembly(pluginAssembly);
            pluginRegistry.PluginContext = pluginContext;
        }

        SavePluginRegistry(pluginRegistries);
        _logger.Debug($"已加载 {pluginRegistries.Count} 个插件。");

        _plugins = pluginRegistries;
        _isInitialized = true;
        return pluginRegistries;
    }
    public Task<List<PluginRegistry>> LoadPluginsAsync()
    {
        return Task.Run(LoadPlugins);
    }
    private void SavePluginRegistry(List<PluginRegistry> pluginRegistries)
    {
        string pluginRegistriesJson = JsonSerializer.Serialize(pluginRegistries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_pluginRegistryPath, pluginRegistriesJson);
    }

    private IPlugin LoadPluginByAssembly(Assembly pluginAssembly)
    {
        var pluginType = pluginAssembly.GetExportedTypes()
            .Where(t => typeof(IPlugin).IsAssignableFrom(t))
            .Where(t => t != typeof(IPlugin))
            .Where(t => !t.IsAbstract)
            .SingleOrDefault();

        ModuleInfo moduleInfo = CreateModuleInfo(pluginType);
        _moduleCatalog.AddModule(moduleInfo);
        _moduleManager.LoadModule(moduleInfo.ModuleName);

        return (IPlugin)Activator.CreateInstance(pluginType);
    }

    private static ModuleInfo CreateModuleInfo(Type type)
    {
        string moduleName = type.Name;

        var moduleAttribute = CustomAttributeData.GetCustomAttributes(type)
            .FirstOrDefault(cad => cad.Constructor.DeclaringType.FullName == typeof(ModuleAttribute).FullName);

        if (moduleAttribute != null)
        {
            foreach (CustomAttributeNamedArgument argument in moduleAttribute.NamedArguments)
            {
                if (argument.MemberInfo.Name == "ModuleName")
                {
                    moduleName = (string)argument.TypedValue.Value;
                    break;
                }
            }
        }

        return new ModuleInfo(moduleName, type.AssemblyQualifiedName)
        {
            InitializationMode = InitializationMode.OnDemand,
            Ref = type.Assembly.CodeBase,
        };
    }
}
