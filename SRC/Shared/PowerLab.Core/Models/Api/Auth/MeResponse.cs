namespace PowerLab.Core.Models.Api.Auth;

public class MeResponse
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
}
