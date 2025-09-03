using System;
using System.IO;
using PowerLab.PluginContracts.Interfaces;

namespace PowerLab.ExternalTools.Data
{
    public class PathProviderDesignTime : IPathProvider
    {
        private string _basePath => 
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerLab");

        public string Plugins => Path.Combine(_basePath, "plugins");
        public string Data => Path.Combine(_basePath, "data");

        public string PluginsRootPath => Plugins;

        public string DataRootPath => Data;
    }
}
