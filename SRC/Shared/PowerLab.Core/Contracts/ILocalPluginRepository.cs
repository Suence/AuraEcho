using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface ILocalPluginRepository
{
    /// <summary>
    /// 获取插件信息列表
    /// </summary>
    /// <returns></returns>
    List<PluginRegistry> GetPluginRegistries();

    /// <summary>
    /// 添加插件信息
    /// </summary>
    /// <param name="pluginRegistry"></param>
    void AddPluginRegistry(PluginRegistry pluginRegistry);

    /// <summary>
    /// 移除插件信息
    /// </summary>
    /// <param name="pluginRegistryId"></param>
    void RemovePluginRegistry(string pluginRegistryId);

    /// <summary>
    /// 更新插件信息
    /// </summary>
    /// <param name="pluginRegistry"></param>
    void UpdatePluginRegistry(PluginRegistry pluginRegistry);
}
