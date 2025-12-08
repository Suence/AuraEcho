namespace PowerLab.Core.Models.Api;

public class CreatePluginVersionRequest
{
    public string PluginId { get; set; }
    public string Version { get; set; }
    public string FileId { get; set; }
}
