using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IClientSession
{
    bool IsSignedIn { get; }
    AppToken? AppToken { get; }

    UserProfile? CurrentUser { get; }
    Task<bool> TryRefreshTokenAsync();
    void SignIn(AppToken appToken);
    void SignOut();
}
