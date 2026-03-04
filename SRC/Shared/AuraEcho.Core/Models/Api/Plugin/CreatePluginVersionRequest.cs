namespace AuraEcho.Core.Models.Api;

public class CreatePluginVersionRequest
{
    public Guid PluginId { get; set; }
    public string Version { get; set; }
    public Guid FileId { get; set; }
}
