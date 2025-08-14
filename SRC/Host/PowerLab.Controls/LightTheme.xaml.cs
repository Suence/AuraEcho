using System.Windows;

namespace PowerLab.Controls
{
    public partial class LightTheme : ResourceDictionary
    {
        private LightTheme() => InitializeComponent();
        public static LightTheme Instance { get; } = new LightTheme();
    }
}
