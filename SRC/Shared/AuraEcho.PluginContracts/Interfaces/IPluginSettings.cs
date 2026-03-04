using AuraEcho.PluginContracts.Models;

namespace AuraEcho.PluginContracts.Interfaces;

public interface IPluginSettings
{
    AppSettingsItem GetSettings();
}
