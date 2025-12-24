namespace PowerLab.Core.Models;

public class AppToken
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; set; }
}
