namespace PowerLab.Core.Models.Api;

public class PluginVersionItem
{
    public Guid Id { get; set; }
    public Guid PluginId { get; set; }

    public string Version { get; set; }
    public bool IsActive { get; set; }
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public long Size { get; set; }
    public bool IsReleased { get; set; }
    public DateTime? CreateTime { get; set; }
}
