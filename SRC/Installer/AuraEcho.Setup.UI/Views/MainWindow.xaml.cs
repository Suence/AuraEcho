using Prism.Events;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AuraEcho.Installer.Bootstrapper.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(IEventAggregator eventAggregator)
    {
        InitializeComponent();
    }

    private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Application.Current.Properties["MainWindowHandle"] = new WindowInteropHelper(this).Handle;

        // 用于在用户点击 UAC 弹窗按钮后将主窗口前置
        BringToForeground();
    }

    /// <summary>
    /// 使主窗口前置
    /// </summary>
    public void BringToForeground()
    {
        Topmost = true;
        Topmost = false;
        Focus();
    }
}
