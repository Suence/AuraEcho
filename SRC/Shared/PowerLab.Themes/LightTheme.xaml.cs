using System.Windows;

namespace PowerLab.Themes
{
    public partial class LightTheme : ResourceDictionary
    {
        private LightTheme() => InitializeComponent();
        public static LightTheme Instance { get; } = new LightTheme();
    }
}
