using System.IO;
using System.Net.Http;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class AppPackageRepository : IAppPackageRepository
{
    private readonly HttpHelper _httpHelper;
    public AppPackageRepository(HttpHelper httpHelper)
    {
        _httpHelper = httpHelper;
    }
    public async Task<Guid?> CreatePackageAsync(Guid fileId, string name, string version)
    {
        var request = new CreatePackageRequest { Name = name, Version = version, FileId = fileId };
        var response = await _httpHelper.PostAsync<CreatePackageResponse>($"{Urls.ServerUrl}/api/package/create", request);
        return response?.PackageId;
    }

    public async Task<bool> DeleteAsync(Guid packageId)
    {
        bool result = await _httpHelper.DeleteAsync($"{Urls.ServerUrl}/api/package/delete/{packageId}");
        return result;
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
        var result = await _httpHelper.GetAsync<GetLatestVersionResponse>($"{Urls.ServerUrl}/api/package/latest");
        if (result is null) return null;

        return new AppVersionInfo
        {
            FileId = result.FileId,
            Version = result.Version,
            FileName = result.FileName,
            FileSize = result.FileSize,
            ReleaseDate = result.ReleaseDate,
        };
    }

    public async Task<List<AppPackageDetail>> GetUploadedPackagesAsync()
    {
        var result = await _httpHelper.GetAsync<ListPackagesResponse>($"{Urls.ServerUrl}/api/package/list");
        if (result is null) return null;

        List<AppPackageDetail> packages =
            result.Packages.Select(p => new AppPackageDetail
            {
                FileName = p.FileName,
                CreateTime = p.CreateTime,
                FileId = p.FileId,
                Id = p.Id,
                Name = p.Name,
                Size = p.Size,
                Version = p.Version,
            }).ToList();
        return packages;
    }
}

