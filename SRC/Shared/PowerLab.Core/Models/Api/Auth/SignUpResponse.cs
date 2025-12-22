namespace PowerLab.Core.Models.Api.Auth;

public class SignUpResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }

    public AppUserDto User { get; set; }
}
