using System.Net.Http;
using System.Net.Http.Json;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api.Auth;
using PowerLab.Core.Tools.HttpClientPipelines;
using PowerLab.PluginContracts.Interfaces;
using Prism.Mvvm;

namespace PowerLab.Core.Services;

public class ClientSession : BindableBase, IClientSession
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly IClock _clock;

    public ClientSession(IClock clock, IAppLogger logger)
    {
        _clock = clock;
        var logHandler = new LoggingHandler(logger);
        logHandler.InnerHandler = new HttpClientHandler();
        _httpClient = new HttpClient(logHandler);
    }

    public bool IsSignedIn => AppToken is not null;

    public AppToken? AppToken { get; private set; }
    
    public UserProfile? CurrentUser
    {
        get;
        private set => SetProperty(ref field, value);
    }

    private bool IsExpired()
    {
        return _clock.UtcNow >= AppToken.ExpiresAt;
    }

    public void UpdateToken(AppToken appToken)
    {
        AppToken = appToken;
    }

    public void SignIn(AppToken appToken)
    {
        AppToken = appToken;
    }

    public void SignOut()
    {
        CurrentUser = null;
        AppToken = null;
    }

    public async Task<bool> TryRefreshTokenAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            if (!IsExpired())
                return true;

            if (string.IsNullOrWhiteSpace(AppToken?.RefreshToken))
                return false;

            var response = await _httpClient.PostAsJsonAsync(
                $"{Urls.ServerUrl}/api/auth/refresh",
                new RefreshTokenRequest
                {
                    RefreshToken = AppToken.RefreshToken
                });
            if (!response.IsSuccessStatusCode)
                return false;

            var token = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
            if (token is null)
                return false;

            UpdateToken(new AppToken
            { 
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresAt = token.ExpiresAt
            });

            return true;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}
