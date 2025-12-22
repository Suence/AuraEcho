using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models.Api;
using PowerLab.Core.Models.Api.Auth;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly IClientSession _clientSession;
    public AuthRepository(IClientSession clientSession)
    {
        _clientSession = clientSession;
    }

    public async Task<MeResponse> GetCurrentUserAsync()
    {
        var apiHelper = new HttpHelper();
        apiHelper.SetToken(_clientSession.AccessToken ?? string.Empty);

        var result = await apiHelper.GetAsync<MeResponse>($"{Urls.ServerUrl}/api/auth/me");

        return result;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var apiHelper = new HttpHelper();
        var result = await apiHelper.PostAsync<SignInResponse>($"{Urls.ServerUrl}/api/auth/signin", request);

        return result;
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
    {
        var apiHelper = new HttpHelper();
        var result = await apiHelper.PostAsync<SignUpResponse>($"{Urls.ServerUrl}/api/auth/signup", request);
        return result;
    }
}
