using System.Windows;
using PowerLab.PluginContracts.Models;

namespace PowerLab.PluginContracts.Interfaces
{
    public interface IPluginTheme
    {
        ResourceDictionary GetThemeResource(AppTheme theme);
    }
}
