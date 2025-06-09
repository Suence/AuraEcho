namespace PowerLab.Host.Core.Models
{
    public class PluginManifest
    {
        public string? Id { get; set; }
        public string? PluginName { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public string? EntryAssemblyName { get; set; }
    }
}
