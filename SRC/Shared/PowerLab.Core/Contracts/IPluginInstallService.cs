using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts
{
    public interface IPluginInstallService
    {
        public Task<PluginRegistry> InstallAsync(string filePath);
    }
}
