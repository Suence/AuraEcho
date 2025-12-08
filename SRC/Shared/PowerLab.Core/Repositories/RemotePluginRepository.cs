using System.IO;
using System.Net.Http;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;
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
        var helper = new HttpHelper();
        var resp = await helper.PostAsync<CreatePluginResponse>($"{Urls.ServerUrl}/api/plugin/create", req);
        if (resp is null) return null;

        return resp.PluginId;
    }

    public async Task<string> CreateVersionAsync(CreatePluginVersionRequest req)
    {
        var helper = new HttpHelper();
        var resp = await helper.PostAsync<CreatePluginVersionResponse>($"{Urls.ServerUrl}/api/plugin/createVersion", req);
        if (resp is null) return null;

        return resp.PackageId;
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
        var result = await httpHelper.GetAsync<GetPluginLatestVersionResponse>($"{Urls.ServerUrl}/api/plugin/latest?pluginId={pluginId}");
        if (result is null) return null;

        return new PluginPackage
        {
            PluginId = result.PluginId,
            FileName = result.FileName,
            FileId = result.FileId,
            CreateTime = result.CreateTime,
            Id = result.Id,
            Size = result.Size,
            Version = result.Version
        };
    }

    public async Task<List<AppPlugin>> GetPluginsAsync()
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<ListPluginsResponse>($"{Urls.ServerUrl}/api/plugin/list");
        if (result is null) return null;

        List<AppPlugin> plugins =
            result.Plugins
                  .Select(p => new AppPlugin
                  {
                      Author = p.Author,
                      Name = p.Name,
                      CreateTime = p.CreateTime,
                      Id = p.Id,
                      Description = p.Description,
                      DisplayName = p.DisplayName,
                      IconFileId = p.IconFileId,
                      IsEnabled = p.IsEnabled
                  })
                  .ToList();

        return plugins;
    }

    public async Task<List<PluginPackage>> GetVersionsAsync(string pluginId)
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<GetPluginVersionsResponse>($"{Urls.ServerUrl}/api/plugin/versions?pluginId={pluginId}");
        if (result is null) return null;

        var pluginVersions =
            result.Versions
                  .Select(v => new PluginPackage
                  {
                      CreateTime = v.CreateTime,
                      Id = v.Id,
                      Version = v.Version,
                      FileId = v.FileId,
                      FileName = v.FileName,
                      PluginId = v.PluginId,
                      Size = v.Size
                  })
                  .ToList();
        return pluginVersions;
    }
}

