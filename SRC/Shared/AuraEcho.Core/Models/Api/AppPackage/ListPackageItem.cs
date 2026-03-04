namespace AuraEcho.Core.Models.Api;

public class ListPackageItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public Guid? FullFileId { get; set; }
    public string? FullFileName { get; set; }
    public long? FullFileSize { get; set; }

    public Guid? UpdateFileId { get; set; }
    public string? UpdateFileName { get; set; }
    public long? UpdateFileSize { get; set; }
    public DateTime CreateTime { get; set; }
}
