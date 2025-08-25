using System.Windows;
using PowerLab.PluginContracts.Models;

namespace PowerLab.PluginContracts.Interfaces
{
    public interface IPluginThemeProvider
    {
        ResourceDictionary GetThemeResource(AppTheme theme);
    }
}
