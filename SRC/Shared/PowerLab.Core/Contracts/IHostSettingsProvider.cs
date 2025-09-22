using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IHostSettingsProvider
{
    HostSettings LoadHostSettings();
    void SaveHostSettings(HostSettings settings);
}
