namespace AuraEcho.Core.Models.Api;

public class UploadFileResponse
{
    public Guid FileId { get; set; }
    public bool IsDuplicated { get; set; }
}
