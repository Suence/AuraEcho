namespace PowerLab.Core.Models;

public class AppVersionInfo
{
    public string Version { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
}
