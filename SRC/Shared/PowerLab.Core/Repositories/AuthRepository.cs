using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models.Api;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class AuthRepository : IAuthRepository
{
    private HttpHelper _httpHelper;

    public AuthRepository(HttpHelper httpHelper)
    {
        _httpHelper = httpHelper;
    }

    public async Task<MeResponse> GetCurrentUserAsync()
    {
        var result = await _httpHelper.GetAsync<MeResponse>($"{Urls.ServerUrl}/api/auth/me");
        return result;
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var result = await _httpHelper.PostAsync<RefreshTokenResponse>($"{Urls.ServerUrl}/api/auth/refresh", request);

        return result;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var result = await _httpHelper.PostAsync<SignInResponse>($"{Urls.ServerUrl}/api/auth/signin", request);

        return result;
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
    {
        var result = await _httpHelper.PostAsync<SignUpResponse>($"{Urls.ServerUrl}/api/auth/signup", request);
        return result;
    }


}
