using AuraEcho.PluginContracts.Models;
using System.Windows;

namespace AuraEcho.PluginContracts.Interfaces;

public interface IPluginTheme
{
    ResourceDictionary GetThemeResource(AppTheme theme);
}
