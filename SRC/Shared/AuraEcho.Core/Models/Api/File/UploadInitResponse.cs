namespace AuraEcho.Core.Models.Api;

public class UploadInitResponse
{
    public Guid UploadId { get; set; }
    public Guid FileId { get; set; }
    public bool IsDuplicated { get; set; }
}
