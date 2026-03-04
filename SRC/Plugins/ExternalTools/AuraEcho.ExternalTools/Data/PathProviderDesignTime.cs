using AuraEcho.PluginContracts.Interfaces;
using System;
using System.IO;

namespace AuraEcho.ExternalTools.Data;

public class PathProviderDesignTime : IPathProvider
{
    private string _basePath => 
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AuraEcho");

    public string Plugins => Path.Combine(_basePath, "plugins");
    public string Data => Path.Combine(_basePath, "data");

    public string PluginsRootPath => Plugins;

    public string DataRootPath => Data;
}
