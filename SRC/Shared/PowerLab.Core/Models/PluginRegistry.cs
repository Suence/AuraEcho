using PowerLab.PluginContracts.Interfaces;
using Prism.Mvvm;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PowerLab.Core.Models;

/// <summary>
/// 模块配置信息
/// </summary>
public class PluginRegistry : BindableBase
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

    [JsonIgnore]
    [NotMapped]
    public IPlugin PluginContext { get; set; }
}
