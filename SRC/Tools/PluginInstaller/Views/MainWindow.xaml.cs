using System.Windows;
using PowerLab.Core.Attributes;

namespace PluginInstaller.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Logging]
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}