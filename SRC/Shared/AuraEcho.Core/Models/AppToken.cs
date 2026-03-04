namespace AuraEcho.Core.Models;

public class AppToken
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTimeOffset ExpiresAt { get; set; }
}
