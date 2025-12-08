namespace PowerLab.Core.Models.Api;

public class ListPluginItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string IconFileId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreateTime { get; set; }
}
