using System.Collections.Generic;
using System.Threading.Tasks;
using PowerLab.Core.Models;

namespace PowerLab.Interfaces
{
    public interface IPluginManager
    {
        List<PluginRegistry> Plugins { get; }

        List<PluginRegistry> LoadPlugins();
        /// <summary>
        /// 加载所有插件并返回插件注册表
        /// </summary>
        Task<List<PluginRegistry>> LoadPluginsAsync();
    }
}
