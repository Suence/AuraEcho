using System.Windows;
using PowerLab.FishyTime.Models;
using Prism.Events;

namespace PowerLab.FishyTime.Events
{
    public class MaskWindowClosedEvent : PubSubEvent<MaskWindowEventArgs>
    {
    }

    public class MaskWindowEventArgs(Rect maskedArea, ManagedWindowInfo windowInfo, WindowMaskMode windowMaskMode)
    {
        public WindowMaskMode WindowMaskMode { get; set; } = windowMaskMode;
        public Rect MaskedArea { get; set; } = maskedArea;
        public ManagedWindowInfo WindowInfo { get; set; } = windowInfo;
    }
}
