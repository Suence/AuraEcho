namespace PowerLab.Core.Models.Api;

public class UploadInitResponse
{
    public string UploadId { get; set; }
    public string FileId { get; set; }
    public bool IsDuplicated { get; set; }
}
