using System;
using Prism.Mvvm;

namespace PowerLab.Core.Models;

public class PluginPackage : BindableBase
{
    public string Id { get; set; }
    public string PluginId { get; set; }

    public string Version { get; set; }

    public string? FileId { get; set; }
    public string FileName { get; set; }
    public long Size { get; set; }
    public DateTime? CreateTime { get; set; }

    private double _progress;
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
}
