using System.IO;
using System.Security.Cryptography;

namespace AuraEcho.Core.Tools;

public static class HashHelper
{
    public static async Task<string> ComputeSha256Async(Stream stream)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }
}
