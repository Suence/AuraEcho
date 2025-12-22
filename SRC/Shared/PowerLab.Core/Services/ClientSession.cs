using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using Prism.Mvvm;

namespace PowerLab.Core.Services;

public class ClientSession : BindableBase, IClientSession
{
    public bool IsSignedIn => AccessToken is not null;

    public string? AccessToken { get; private set; }

    public UserProfile? CurrentUser
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public void SignIn(string accessToken, UserProfile userProfile)
    {
        CurrentUser = userProfile;
        AccessToken = accessToken;
    }

    public void SignOut()
    {
        CurrentUser = null;
        AccessToken = null;
    }
}
