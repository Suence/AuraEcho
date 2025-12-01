using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IPluginRespository
{
    Task<List<AppPlugin>> GetPluginsAsync();
    Task<string> CreatePluginAsync(CreatePluginRequest req);
    Task<string> CreateVersionAsync(CreatePluginVersionRequest req);
    Task<List<PluginPackage>> GetVersionsAsync(string pluginId);
    Task<PluginPackage> GetLatestAsync(string pluginId);
    Task<bool> DownloadLatestAsync(string pluginId, string build, string outputPath, IProgress<double> progress);
    Task<bool> DeleteAsync(string pluginId);
    Task<bool> DeleteVersionAsync(string versionId);
}
