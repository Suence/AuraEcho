using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IClientSession
{
    bool IsSignedIn { get; }
    string? AccessToken { get; }

    UserProfile? CurrentUser { get; }
    
    void SignIn(string accessToken, UserProfile userProfile);
    void SignOut();
}
