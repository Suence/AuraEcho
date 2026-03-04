using AuraEcho.Core.Models;
using AuraEcho.Services;
using Prism.Mvvm;

namespace AuraEcho.Models;

public class MarketPlugin : BindableBase
{
    public AppPlugin PluginInfo { get; set; }
    public PluginDownloadTask InstallContext { get; set; }
}
