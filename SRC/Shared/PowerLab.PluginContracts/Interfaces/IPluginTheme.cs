using PowerLab.PluginContracts.Models;
using System.Windows;

namespace PowerLab.PluginContracts.Interfaces;

public interface IPluginTheme
{
    ResourceDictionary GetThemeResource(AppTheme theme);
}
