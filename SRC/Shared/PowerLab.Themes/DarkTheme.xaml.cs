using System.Windows;

namespace PowerLab.Themes
{
    public partial class DarkTheme : ResourceDictionary
    {
        private DarkTheme() => InitializeComponent();
        public static DarkTheme Instance { get; } = new DarkTheme();
    }
}
