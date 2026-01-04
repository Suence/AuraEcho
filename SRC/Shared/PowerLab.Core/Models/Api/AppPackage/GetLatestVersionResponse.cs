namespace PowerLab.Core.Models.Api;

public class GetLatestVersionResponse
{
    public string Version { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ReleaseDate { get; set; }
}
