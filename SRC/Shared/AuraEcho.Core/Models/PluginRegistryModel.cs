using AuraEcho.PluginContracts.Interfaces;
using Prism.Mvvm;

namespace AuraEcho.Core.Models;

/// <summary>
/// 模块配置信息
/// </summary>
public class PluginRegistryModel : BindableBase
{
    public string Id { get; set; }

    /// <summary>
    /// 计划状态
    /// </summary>
    public PluginPlanStatus PlanStatus
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// 清单信息
    /// </summary>
    public PluginManifest Manifest
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// 模块所在目录路径
    /// </summary>
    public string PluginFolder { get; set; } = string.Empty;

    public IPlugin PluginContext { get; set; }
}
