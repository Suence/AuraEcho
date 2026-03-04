using Prism.Mvvm;

namespace AuraEcho.FishyTime.Models;

public class MouseLeaveMaskConfig : BindableBase
{
    private WindowMaskStyleConfig _windowMaskStyleConfig;
    public WindowMaskStyleConfig WindowMaskStyleConfig
    {
        get => _windowMaskStyleConfig;
        set => SetProperty(ref _windowMaskStyleConfig, value);
    }
}
