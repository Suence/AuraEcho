using System;
using System.Windows;
using PowerLab.FishyTime.Events;
using Prism.Events;

namespace PowerLab.FishyTime.Views
{
    /// <summary>
    /// SpotlightWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpotlightWindow : Window
    {
        #region private members
        private readonly IEventAggregator _eventAggregator;
        #endregion
        public SpotlightWindow(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            InitializeComponent();
        }


        private void UpdateSpotlight(Point mousePosition)
        {
            PresentationSource source = PresentationSource.FromVisual(RootGrid);
            if (source is null) return;

            // 将屏幕坐标转换为 Grid 的相对坐标
            Point relativePoint = RootGrid.PointFromScreen(mousePosition);

            double x = relativePoint.X / RootGrid.ActualWidth;
            double y = relativePoint.Y / RootGrid.ActualHeight;

            //x = Math.Clamp(x, 0.0, 1.0);
            //y = Math.Clamp(y, 0.0, 1.0);

            SpotlightBrush.Center = new Point(x, y);
            SpotlightBrush.GradientOrigin = new Point(x, y);
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            double w = RootGrid.ActualWidth;
            double h = RootGrid.ActualHeight;

            double radiusX = Math.Min(0.5, h / (2 * w)); // = min(0.5, h / 2w)
            double radiusY = Math.Min(w / (2 * h), 0.5); // = min(w / 2h, 0.5)

            SpotlightBrush.RadiusX = radiusX;
            SpotlightBrush.RadiusY = radiusY;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<MouseMoveEvent>().Subscribe(UpdateSpotlight, ThreadOption.UIThread);
            _eventAggregator.GetEvent<WindowMaskDisabledEvent>().Subscribe(Close, ThreadOption.UIThread);
            _eventAggregator.GetEvent<SpotlightWindowCloseRequestedEvent>().Subscribe(Close, ThreadOption.UIThread);
        }
    }
}
