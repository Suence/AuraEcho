using Prism.Events;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace PowerLab.Installer.Bootstrapper.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            //eventAggregator.GetEvent<InstallationStartedEvent>().Subscribe(InstallationStarted);
        }

        private void InstallationStarted()
        {
            Foreground = new SolidColorBrush(Colors.White);
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
        }
    }
}
