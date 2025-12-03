using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class AppPackageRepository : IAppPackageRepository
{
    private readonly HttpClient _client;

    public AppPackageRepository()
    {
        _client = new HttpClient();
    }
    public async Task<string> CreatePackageAsync(string fileId, string name, string version)
    {
        var dto = new { Name = name, Version = version, FileId = fileId };
        var resp = await _client.PostAsJsonAsync($"{Urls.ServerUrl}/api/package/create", dto);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("packageId").GetString()!;
    }

    public async Task<bool> DeleteAsync(string packageId)
    {
        try
        {
            var resp = await _client.DeleteAsync($"{Urls.ServerUrl}/api/package/delete/{packageId}");
            resp.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DownloadLatestAsync(string build, string outputPath, IProgress<double> progress)
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync($"{Urls.ServerUrl}/api/package/download?build={build}", HttpCompletionOption.ResponseHeadersRead);
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

    public async Task<AppVersionInfo> GetLatestAsync()
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<AppVersionInfo>($"{Urls.ServerUrl}/api/package/latest");
        return result;
    }

    public async Task<List<AppPackageDetail>> GetUploadedPackagesAsync()
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<List<AppPackageDetail>>($"{Urls.ServerUrl}/api/package/list");
        return result;
    }
}

