using Prism.Mvvm;

namespace PowerLab.FishyTime.Models;

public class WindowMaskStyleConfig : BindableBase
{
    private string _backgroundImagePath;
    public string BackgroundImagePath
    {
        get => _backgroundImagePath;
        set => SetProperty(ref _backgroundImagePath, value);
    }
}
