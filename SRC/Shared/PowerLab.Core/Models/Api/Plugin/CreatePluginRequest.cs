namespace PowerLab.Core.Models.Api;

public class CreatePluginRequest
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public string IconFileId { get; set; }
}
