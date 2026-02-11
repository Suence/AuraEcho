using System.Windows;
using System.Windows.Controls;
using PowerLab.Installer.Bootstrapper.ViewModels;

namespace PowerLab.Installer.Bootstrapper.Views;

/// <summary>
/// Interaction logic for InstallFinish
/// </summary>
public partial class InstallFinish : UserControl
{
    public InstallFinish()
    {
        InitializeComponent();
    }

    private void FinishedButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Application.Current.MainWindow.Close();

        (DataContext as InstallFinishViewModel)!.FinishedCommand.Execute();
    }
}
