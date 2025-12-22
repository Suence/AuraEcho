using PowerLab.Core.Models.Api.Auth;

namespace PowerLab.Core.Models.Api;

public class SignInResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public AppUserDto User { get; set; }
}
