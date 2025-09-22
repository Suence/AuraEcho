using PowerLab.FishyTime.Contracts;
using PowerLab.FishyTime.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PowerLab.FishyTime.Views;

/// <summary>
/// MaskWindow.xaml 的交互逻辑
/// </summary>
public partial class MaskWindow : Window, IWindowMask
{
    private readonly Win32Window _win32Window;

    public nint Handle => new WindowInteropHelper(this).Handle;

    public MaskWindow(Win32Window win32Window)
    {
        _win32Window = win32Window;
        InitializeComponent();
    }

    public event Action MaskClosed;

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        MaskClosed?.Invoke();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (_win32Window.IsClosed) return;

        _win32Window.Position = RestoreBounds.Location;
        _win32Window.Width = RestoreBounds.Width;
        _win32Window.Height = RestoreBounds.Height;
        _win32Window.Show();
    }

    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle) return;

        base.Close();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        DragMove();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Top = _win32Window.Position.Y;
        Left = _win32Window.Position.X;
        Width = _win32Window.Width;
        Height = _win32Window.Height;

        _win32Window.Hide();
    }

}
