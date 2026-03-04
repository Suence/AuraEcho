using System.Text.Json.Serialization;
using Prism.Mvvm;

namespace AuraEcho.Core.Models;

public class AppPackageDetail : BindableBase
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }

    public Guid? FullFileId { get; set; }
    public string? FullFileName { get; set; }
    public long? FullFileSize { get; set; }

    public Guid? UpdateFileId { get; set; }
    public string? UpdateFileName { get; set; }
    public long? UpdateFileSize { get; set; }

    public DateTime CreateTime { get; set; }
    public bool IsActive
    {
        get;
        set => SetProperty(ref field, value);
    }
    private double _progress;

    [JsonIgnore]
    public double Progress
    {
        get { return _progress; }
        set { SetProperty(ref _progress, value); }
    }
}
