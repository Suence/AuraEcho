using PowerLab.Core.Constants;
using PowerLab.PluginContracts.Interfaces;

namespace PowerLab.Core.Services;

public class PathProvider : IPathProvider
{
    public string PluginsRootPath => ApplicationPaths.Plugins;

    public string DataRootPath => ApplicationPaths.Data;
}
