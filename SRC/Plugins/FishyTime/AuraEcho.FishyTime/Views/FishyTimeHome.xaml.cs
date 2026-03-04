using AuraEcho.FishyTime.Utils;
using AuraEcho.FishyTime.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WinForms = System.Windows.Forms;
namespace AuraEcho.FishyTime.Views;

/// <summary>
/// Interaction logic for FishyTimeHome
/// </summary>
public partial class FishyTimeHome : UserControl
{
    private readonly List<Window> _blackWindows = [];
    private Window _ownerWindow;

    public FishyTimeHome()
    {
        InitializeComponent();
    }

    private void OpenWindowsUpdateWindowButton_Click(object sender, RoutedEventArgs e)
    {
        CloseAllFullScreenWindows();

        foreach (var screen in WinForms::Screen.AllScreens)
        {
            Window fullScreenWindow =
                screen.Primary
                ? new WindowsUpdateWindow()
                : new Window();
            fullScreenWindow.WindowStyle = WindowStyle.None;
            fullScreenWindow.ResizeMode = ResizeMode.NoResize;
            fullScreenWindow.Left = screen.Bounds.Left;
            fullScreenWindow.Top = screen.Bounds.Top;
            fullScreenWindow.Width = screen.Bounds.Width;
            fullScreenWindow.Height = screen.Bounds.Height;
            fullScreenWindow.Topmost = true;
            fullScreenWindow.Cursor = Cursors.None;
            fullScreenWindow.Background = Brushes.Black;
            fullScreenWindow.ShowInTaskbar = false;

            fullScreenWindow.InputBindings.Add(new InputBinding(ApplicationCommands.Close, new KeyGesture(Key.Escape)));
            fullScreenWindow.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, FullScreenWindowEsc_Click));
            fullScreenWindow.Closed += FullScreenWindowClosed;
            _blackWindows.Add(fullScreenWindow);
            fullScreenWindow.Show();
        }
    }

    private void FullScreenWindowEsc_Click(object sender, ExecutedRoutedEventArgs e) => CloseAllFullScreenWindows();
    private void FullScreenWindowClosed(object sender, EventArgs e)
        => CloseAllFullScreenWindows();
    private void CloseAllFullScreenWindows()
    {
        if (_blackWindows.Count == 0) return;

        _blackWindows.ToList().ForEach(window => window.Close());
        _blackWindows.Clear();
    }


    private void PickButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture((IInputElement)sender);
        Cursor = Cursors.Cross;
    }

    private void PickButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(null);
        Cursor = Cursors.Arrow;

        // 获取鼠标所在窗口句柄
        var hwnd = Win32Helper.GetTopLevelWindowUnderMouse();
        if (hwnd == IntPtr.Zero) return;
        
        var ownerWindowHandle = new WindowInteropHelper(_ownerWindow).Handle;
        if (hwnd == ownerWindowHandle) return;

        (DataContext as FishyTimeHomeViewModel).AddWin32WindowCommand.Execute(hwnd);
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _ownerWindow ??= Window.GetWindow(this);
    }
}
