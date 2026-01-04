using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;

namespace PowerLab.Core.Contracts;

public interface IAppPackageRepository
{
    Task<Guid?> CreatePackageAsync(Guid fileId, string name, string version);
    Task<List<AppPackageDetail>> GetUploadedPackagesAsync();
    Task<AppVersionInfo> GetLatestAsync();
    Task<bool> DownloadLatestAsync(string build, string outputPath, IProgress<double> progress);
    Task<bool> DeleteAsync(Guid packageId);
}
