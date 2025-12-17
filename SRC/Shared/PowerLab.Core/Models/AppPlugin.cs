using Prism.Mvvm;

namespace PowerLab.Core.Models;

public class AppPlugin : BindableBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string IconFileId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public bool IsInstalled
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    private List<PluginPackage> _versions;
    public List<PluginPackage> Versions 
    {
        get => _versions;
        set => SetProperty(ref _versions, value);
    }
}
