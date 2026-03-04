using AuraEcho.ExternalTools.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AuraEcho.ExternalTools.Utils;

public static class ShellHelper
{
    public static bool IsShellExecutable(string input)
        => CheckExternalToolType(input) != ExternalToolType.None;

    public static ExternalToolType CheckExternalToolType(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ExternalToolType.None;

        // Url
        if (Uri.TryCreate(input, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeMailto))
            return ExternalToolType.Website;

        // File
        if (File.Exists(input) || Directory.Exists(input))
            return ExternalToolType.File;

        // 简单判断可执行命令（like "notepad"）
        return IsExecutableWithoutPath(input) ? ExternalToolType.PathCommand : ExternalToolType.None;
    }

    public static async Task<ImageSource> GetWebSiteIconAsync(string url)
    {
        Uri uri = new UriBuilder(url).Uri;
        string faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";

        using var httpClient = new HttpClient();
        var data = await httpClient.GetByteArrayAsync(faviconUrl);

        using var stream = new MemoryStream(data);
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        return decoder.Frames[0];
    }

    public static ImageSource GetFaviconIcon(string url, int timeoutMilliseconds = 3000)
    {
        try
        {
            var task = GetWebSiteIconAsync(url);
            if (task.Wait(timeoutMilliseconds))
            {
                return task.Result;
            }
            // 超时
            return new BitmapImage(new Uri("pack://application:,,,/AuraEcho.ExternalTools;component/Assets/Images/web.png"));
        }
        catch
        {
            // 网络错误或格式错误
            return new BitmapImage(new Uri("pack://application:,,,/AuraEcho.ExternalTools;component/Assets/Images/web.png"));
        }
    }



    public static bool IsExecutableWithoutPath(string command)
    {
        string[] pathext = Environment.GetEnvironmentVariable("PATHEXT")?.Split(';') ?? [];
        string[] pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? [];

        foreach (var dir in pathDirs)
        {
            foreach (var ext in pathext)
            {
                var fullPath = Path.Combine(dir, command + ext.ToLower());
                if (File.Exists(fullPath))
                    return true;
            }
        }

        return false;
    }

}
