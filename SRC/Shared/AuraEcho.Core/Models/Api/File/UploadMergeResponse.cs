namespace AuraEcho.Core.Models.Api;

public class UploadMergeResponse
{
    public Guid FileId { get; set; }
    public bool IsDuplicated { get; set; }
}
