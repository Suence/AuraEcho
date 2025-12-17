using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IPluginInstallService
{
    public Task<PluginRegistryModel> InstallAsync(string filePath);
}
