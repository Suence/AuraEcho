using System.Collections.Generic;
using System.Threading.Tasks;
using PowerLab.Core.Models;

namespace PowerLab.Interfaces;

public interface IPluginManager
{
    List<PluginRegistryModel> Plugins { get; }

    List<PluginRegistryModel> LoadPlugins();
    Task<bool> LoadPluginAsync(PluginRegistryModel pluginRegistryModel);
    /// <summary>
    /// 加载所有插件并返回插件注册表
    /// </summary>
    Task<List<PluginRegistryModel>> LoadPluginsAsync();
}
