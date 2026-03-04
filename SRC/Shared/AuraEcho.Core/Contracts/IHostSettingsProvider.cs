using AuraEcho.Core.Models;

namespace AuraEcho.Core.Contracts;

public interface IHostSettingsProvider
{
    HostSettings LoadHostSettings();
    void SaveHostSettings(HostSettings settings);
}
