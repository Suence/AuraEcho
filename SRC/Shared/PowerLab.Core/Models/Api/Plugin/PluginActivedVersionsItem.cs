namespace PowerLab.Core.Models.Api;

public class PluginActivedVersionsItem
{
    public string Id { get; set; }
    public string PluginId { get; set; }

    public string Version { get; set; }

    public string? FileId { get; set; }
    public string FileName { get; set; }
    public long Size { get; set; }
    public DateTime? CreateTime { get; set; }
}
