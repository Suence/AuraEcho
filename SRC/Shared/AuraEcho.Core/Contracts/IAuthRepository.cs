using AuraEcho.Core.Models.Api;

namespace AuraEcho.Core.Contracts;

public interface IAuthRepository
{
    Task<SignUpResponse> SignUpAsync(SignUpRequest request);
    Task<SignInResponse> SignInAsync(SignInRequest request);
    Task<MeResponse> GetCurrentUserAsync();
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
