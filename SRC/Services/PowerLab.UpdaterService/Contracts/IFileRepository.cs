using PowerLab.UpdaterService.Models;

namespace PowerLab.UpdaterService.Contracts
{
    public interface IFileRespository
    {
        Task<bool> DownloadFileAsync(string fileId, string outputPath, IProgress<double> progress);
    }
}
