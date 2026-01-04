using System.Text.Json.Serialization;
using Prism.Mvvm;

namespace PowerLab.Core.Models;

public class AppPackageDetail : BindableBase
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FileName { get; set; }
    public string Version { get; set; }
    public Guid FileId { get; set; }
    public long Size { get; set; }
    public DateTime CreateTime { get; set; }

    private double _progress;

    [JsonIgnore]
    public double Progress
    {
        get { return _progress; }
        set { SetProperty(ref _progress, value); }
    }
}
