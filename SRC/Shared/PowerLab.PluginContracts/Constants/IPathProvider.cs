namespace PowerLab.PluginContracts.Constants
{
    public interface IPathProvider
    {
        string PluginsRootPath { get; }
        string DataRootPath { get; }
    }
}
