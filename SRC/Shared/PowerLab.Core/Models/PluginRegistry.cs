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

    private PluginPlanStatus _planStatus;
    /// <summary>
    /// 计划状态
    /// </summary>
    public PluginPlanStatus PlanStatus
    {
        get => _planStatus; 
        set => SetProperty(ref _planStatus, value);
    }

    private PluginManifest _manifest;
    /// <summary>
    /// 清单信息
    /// </summary>
    public PluginManifest Manifest
    {
        get => _manifest;
        set => SetProperty(ref _manifest, value);
    }

    private string _defaultView;
    /// <summary>
    /// 默认视图
    /// </summary>
    public string DefaultView
    {
        get => _defaultView;
        set => SetProperty(ref _defaultView, value);
    }

    /// <summary>
    /// 模块所在目录路径
    /// </summary>
    public string PluginFolder { get; set; } = string.Empty;

    [JsonIgnore]
    [NotMapped]
    public IPlugin PluginContext { get; set; }
}
