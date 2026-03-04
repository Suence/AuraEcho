using AuraEcho.Core.Attributes;
using System.Windows;

namespace PluginInstaller.Views;

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