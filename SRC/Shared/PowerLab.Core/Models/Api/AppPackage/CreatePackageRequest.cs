namespace PowerLab.Core.Models.Api;

public class CreatePackageRequest
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string FileId { get; set; } = "";
}
