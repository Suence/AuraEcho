using System.Windows;

namespace PowerLab.FishyTime.Views
{
    /// <summary>
    /// Interaction logic for WindowsUpdateWindow.xaml
    /// </summary>
    public partial class WindowsUpdateWindow : Window
    {
        public WindowsUpdateWindow()
        {
            InitializeComponent();
            SetWindowFullScreen();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SetWindowFullScreen()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Width = screenWidth;
            Height = screenHeight;
            Left = 0;
            Top = 0;
        }

        private void CloseCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => Close();
    }
}
