namespace PowerLab.Core.Models.Api;

public class ListPackageItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public string FileName { get; set; }
    public Guid FileId { get; set; }
    public long Size { get; set; }
    public DateTime CreateTime { get; set; }
}
