using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;

namespace PowerLab.Core.Contracts;

public interface IRemotePluginRepository
{
    Task<List<AppPlugin>> GetPluginsAsync();
    Task<Guid?> CreatePluginAsync(CreatePluginRequest req);
    Task<Guid?> CreateVersionAsync(CreatePluginVersionRequest req);
    Task<List<PluginPackage>> GetVersionsAsync(Guid pluginId);
    Task<PluginPackage> GetLatestAsync(Guid pluginId);
    Task<bool> DownloadLatestAsync(Guid pluginId, string build, string outputPath, IProgress<double> progress);
    Task<bool> DeleteAsync(Guid pluginId);
    Task<bool> DeleteVersionAsync(Guid versionId);
}
