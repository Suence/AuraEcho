using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class RemotePluginRepository : IRemotePluginRepository
{
    private readonly HttpClient _client;

    public RemotePluginRepository()
    {
        _client = new HttpClient();
    }
    public async Task<string> CreatePluginAsync(CreatePluginRequest req)
    {
        var resp = await _client.PostAsJsonAsync($"{Urls.ServerUrl}/api/plugin/create", req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("pluginId").GetString()!;
    }

    public async Task<string> CreateVersionAsync(CreatePluginVersionRequest req)
    {
        var resp = await _client.PostAsJsonAsync($"{Urls.ServerUrl}/api/plugin/createVersion", req);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("packageId").GetString()!;
    }

    public async Task<bool> DeleteAsync(string pluginId)
    {
        try
        {
            var resp = await _client.DeleteAsync($"{Urls.ServerUrl}/api/plugin/delete/{pluginId}");
            resp.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteVersionAsync(string versionId)
    {
        try
        {
            var resp = await _client.DeleteAsync($"{Urls.ServerUrl}/api/plugin/deleteVersion/{versionId}");
            resp.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DownloadLatestAsync(string pluginId, string build, string outputPath, IProgress<double> progress)
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync($"{Urls.ServerUrl}/api/plugin/download?pluginId={pluginId}&build={build}", HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[80 * 1024];
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;

                if (totalBytes > 0)
                {
                    double percent = totalRead * 100.0 / totalBytes;
                    progress.Report(percent);
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<PluginPackage> GetLatestAsync(string pluginId)
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<PluginPackage>($"{Urls.ServerUrl}/api/plugin/latest?pluginId={pluginId}");
        return result;
    }

    public async Task<List<AppPlugin>> GetPluginsAsync()
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<List<AppPlugin>>($"{Urls.ServerUrl}/api/plugin/list");
        return result;
    }

    public async Task<List<PluginPackage>> GetVersionsAsync(string pluginId)
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<List<PluginPackage>>($"{Urls.ServerUrl}/api/plugin/versions?pluginId={pluginId}");
        return result;
    }
}

