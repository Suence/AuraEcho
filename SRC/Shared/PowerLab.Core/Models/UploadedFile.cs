using System.Text.Json.Serialization;
using Prism.Mvvm;

namespace PowerLab.Core.Models;

public class UploadedFile : BindableBase
{
    public string Id { get; set; } 
    public string FileName { get; set; }
    public string RelativePath { get; set; } 
    public long Size { get; set; }
    public string? MimeType { get; set; }
    public DateTime UploadTime { get; set; }

    private double _progress;
    [JsonIgnore]
    public double Progress 
    { 
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
}
