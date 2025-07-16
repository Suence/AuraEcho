using Prism.Mvvm;

namespace PowerLab.FishyTime.Models
{
    public class SpotlightMaskConfig : BindableBase
    {
        private double _spotlightRadius;

        public double SpotlightRadius
        {
            get => _spotlightRadius;
            set => SetProperty(ref _spotlightRadius, value);
        }

        private WindowMaskStyleConfig _windowMaskStyleConfig;
        public WindowMaskStyleConfig WindowMaskStyleConfig
        {
            get => _windowMaskStyleConfig;
            set => SetProperty(ref _windowMaskStyleConfig, value);
        }
    }
}
