using PowerLab.PluginContracts.Models;

namespace PowerLab.UIToolkit.RegionDialog;

public interface IRegionDialogAware
{
    event Action<RegionDialogResult> RequestClose;

    void OnDialogOpened(RegionDialogParameter parameters);
}
