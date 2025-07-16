using System.Windows;
using System.Windows.Input;
using PowerLab.FishyTime.Events;
using Prism.Events;

namespace PowerLab.FishyTime.Views
{
    /// <summary>
    /// MaskWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MaskWindow : Window
    {
        private readonly IEventAggregator _eventAggregator;

        public MaskWindow(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<WindowMaskDisabledEvent>().Subscribe(Close, ThreadOption.UIThread);
            _eventAggregator.GetEvent<MaskWindowCloseRequestedEvent>().Subscribe(Close, ThreadOption.UIThread);
            InitializeComponent();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle) return;

            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            DragMove();
        }
    }
}
