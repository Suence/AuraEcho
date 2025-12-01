using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IAppPackageRespository
{
    Task<string> CreatePackageAsync(string fileId, string name, string version);
    Task<List<AppPackageDetail>> GetUploadedPackagesAsync();
    Task<AppVersionInfo> GetLatestAsync();
    Task<bool> DownloadLatestAsync(string build, string outputPath, IProgress<double> progress);
    Task<bool> DeleteAsync(string packageId);
}
