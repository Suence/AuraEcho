namespace PowerLab.Core.Models.Api;

public class CreatePackageRequest
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";

    public Guid FullFileId { get; set; }
    public Guid UpdateFileId { get; set; }
}
