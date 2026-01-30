using PowerLab.Core.Models;
using PowerLab.Services;
using Prism.Mvvm;

namespace PowerLab.Models;

public class MarketPlugin : BindableBase
{
    public AppPlugin PluginInfo { get; set; }
    public PluginDownloadTask InstallContext { get; set; }
}
