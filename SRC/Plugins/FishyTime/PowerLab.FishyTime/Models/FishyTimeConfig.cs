using Prism.Mvvm;

namespace PowerLab.FishyTime.Models;

public class FishyTimeConfig : BindableBase
{
    private bool _isWindowMaskEnabled;
    public bool IsWindowMaskEnabled
    {
        get => _isWindowMaskEnabled;
        set => SetProperty(ref _isWindowMaskEnabled, value);
    }

    private WindowMaskMode _windowMaskMode;
    public WindowMaskMode WindowMaskMode
    {
        get => _windowMaskMode;
        set => SetProperty(ref _windowMaskMode, value);
    }

    private MouseLeaveMaskConfig _mouseLeaveMaskConfig;
    public MouseLeaveMaskConfig MouseLeaveMaskConfig
    {
        get => _mouseLeaveMaskConfig;
        set => SetProperty(ref _mouseLeaveMaskConfig, value);
    }

    private HotZoneMaskConfig _hotZoneMaskConfig;
    public HotZoneMaskConfig HotZoneMaskConfig
    {
        get => _hotZoneMaskConfig;
        set => SetProperty(ref _hotZoneMaskConfig, value);
    }

    private SpotlightMaskConfig _spotlightMaskConfig;
    public SpotlightMaskConfig SpotlightMaskConfig
    {
        get => _spotlightMaskConfig;
        set => SetProperty(ref _spotlightMaskConfig, value);
    }
}
