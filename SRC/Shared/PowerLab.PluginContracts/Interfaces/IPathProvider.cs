namespace PowerLab.PluginContracts.Interfaces;

public interface IPathProvider
{
    string PluginsRootPath { get; }
    string DataRootPath { get; }
}
