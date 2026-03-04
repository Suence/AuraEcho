using AuraEcho.FishyTime.Contracts;
using AuraEcho.FishyTime.Models;
using AuraEcho.FishyTime.Utils;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace AuraEcho.FishyTime.Views;

/// <summary>
/// SpotlightWindow.xaml 的交互逻辑
/// </summary>
public partial class SpotlightWindow : Window, IWindowMask
{
    #region private members
    private Win32Window _win32Window;

    public event Action MaskClosed;
    public nint Handle => new WindowInteropHelper(this).Handle;
    #endregion

    public SpotlightWindow(Win32Window win32Window)
    {
        _win32Window = win32Window ?? throw new ArgumentNullException(nameof(win32Window));
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        MaskClosed?.Invoke();
    }

    private void UpdateSpotlight(Point mousePosition) => Dispatcher.BeginInvoke(() =>
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
    });

    private void UpdateSpotlightBrushRadius()
    {
        double w = ActualWidth;
        double h = ActualHeight;

        double radiusX = Math.Min(0.5, h / (2 * w)); // = min(0.5, h / 2w)
        double radiusY = Math.Min(w / (2 * h), 0.5); // = min(w / 2h, 0.5)

        SpotlightBrush.RadiusX = radiusX;
        SpotlightBrush.RadiusY = radiusY;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Top = _win32Window.Position.Y;
        Left = _win32Window.Position.X;
        Width = _win32Window.Width;
        Height = _win32Window.Height;

        _win32Window.MouseMove += UpdateSpotlight;
        _win32Window.RectChanged += ManagedWindowRectChanged;

        UpdateSpotlightBrushRadius();

        Win32Helper.SetWindowOwner(Handle, _win32Window.Handle);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        _win32Window.MouseMove -= UpdateSpotlight;
        _win32Window.Activated -= ManagedWindowActivated;
        _win32Window.Deactivated -= ManagedWindowDeactivated;
        _win32Window.RectChanged -= ManagedWindowRectChanged;
    }

    private void ManagedWindowRectChanged(Rect rect)
    {
        Dispatcher.BeginInvoke(UpdateRectAndBrushRadius);

        void UpdateRectAndBrushRadius()
        {
            Left = rect.Left;
            Top = rect.Top;
            Width = rect.Width;
            Height = rect.Height;
            UpdateSpotlightBrushRadius();
        }
    }

    private void ManagedWindowActivated()
    {
        Debug.WriteLine("Tomost: true");
        Topmost = true;
    }

    private void ManagedWindowDeactivated()
    {
        Debug.WriteLine("Tomost: false");
        Topmost = false;
    }
}
