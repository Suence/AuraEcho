namespace PowerLab.Core.Models.Api;

public class UploadFileListItem
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string RelativePath { get; set; }
    public long Size { get; set; }
    public string? Type { get; set; }
    public DateTime UploadTime { get; set; }
}
