using PowerLab.PluginContracts.Models;

namespace PowerLab.UIToolkit.ContentDialog
{
    public interface IRegionDialogAware
    {
        event Action<RegionDialogResult> RequestClose;

        void OnDialogOpened(RegionDialogParameter parameters);
    }
}
