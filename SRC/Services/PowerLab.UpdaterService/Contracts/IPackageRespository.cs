using PowerLab.UpdaterService.Models;

namespace PowerLab.UpdaterService.Contracts
{
    public interface IPackageRespository
    {
        Task<AppVersionInfo> GetLatestAsync();
        Task<bool> DownloadLatestAsync(string build, string outputPath, IProgress<double> progress);
    }
}
