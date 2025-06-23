using System.Windows.Controls;

namespace PowerLab.FishyTime.Views
{
    /// <summary>
    /// Interaction logic for FishyTimeHome
    /// </summary>
    public partial class FishyTimeHome : UserControl
    {
        public FishyTimeHome()
        {
            InitializeComponent();
        }

        private void OpenWindowsUpdateWindowButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            new WindowsUpdateWindow().Show();
        }
    }
}
