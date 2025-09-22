using PowerLab.Installer.Bootstrapper.ViewModels;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PowerLab.Installer.Bootstrapper.Views;

/// <summary>
/// Interaction logic for Installing
/// </summary>
public partial class Installing : UserControl
{
    public Installing()
    {
        InitializeComponent();
    }

    private Storyboard _progressStoryboard => (Storyboard)FindResource("ProgressStorybard");

    public void BeginProgressStoryboard()
    {
        var newProgress = (DataContext as InstallingViewModel).Progress;
        _progressStoryboard.Begin();
        InstallProgressBar.Value = newProgress;
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        _progressStoryboard.Begin();
        _progressStoryboard.Stop();
    }
}
