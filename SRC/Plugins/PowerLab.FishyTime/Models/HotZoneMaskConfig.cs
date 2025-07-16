using Prism.Mvvm;

namespace PowerLab.FishyTime.Models
{
    public class HotZoneMaskConfig : BindableBase
    {
        private WindowMaskStyleConfig _windowMaskStyleConfig;
        public WindowMaskStyleConfig WindowMaskStyleConfig
        {
            get => _windowMaskStyleConfig;
            set => SetProperty(ref _windowMaskStyleConfig, value);
        }
    }
}
