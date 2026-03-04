namespace AuraEcho.Core.Models.Api;

public class ListAllPluginsResponseItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public Guid IconFileId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreateTime { get; set; }
}
