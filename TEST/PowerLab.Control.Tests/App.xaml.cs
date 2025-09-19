using PowerLab.Themes;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PowerLab.Control.Tests
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //Resources.MergedDictionaries.Add(DarkTheme.Instance);
            base.OnStartup(e);
        }
    }

}
