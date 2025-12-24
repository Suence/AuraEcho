using PowerLab.Core.Models.Api;
using PowerLab.Core.Models.Api.Auth;

namespace PowerLab.Core.Contracts;

public interface IAuthRepository
{
    Task<SignUpResponse> SignUpAsync(SignUpRequest request);
    Task<SignInResponse> SignInAsync(SignInRequest request);
    Task<MeResponse> GetCurrentUserAsync();
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
