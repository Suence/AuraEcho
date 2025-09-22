using System.Windows;
using System.Windows.Input;

namespace PowerLab.Installer.Bootstrapper.Views;

/// <summary>
/// InstallerAlreadyRunningDialog.xaml 的交互逻辑
/// </summary>
public partial class InstallerAlreadyRunningDialog : Window
{
    public InstallerAlreadyRunningDialog()
    {
        InitializeComponent();
    }

    private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
