using PowerLab.PluginContracts.Models;

namespace PowerLab.PluginContracts.Interfaces;

public interface IRegionDialogService
{
    Task<RegionDialogResult> ShowDialogAsync(string regionName, string target, RegionDialogParameter parameter);
}
