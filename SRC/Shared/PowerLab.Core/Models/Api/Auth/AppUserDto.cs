namespace PowerLab.Core.Models.Api.Auth;

public class AppUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
}
