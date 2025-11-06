using PowerLab.UpdaterService.Models;

namespace PowerLab.UpdaterService.Contracts
{
    public interface IFileRespository
    {
        Task<bool> DownloadFileAsync(string url, string outputPath, IProgress<double> progress);
        Task<List<ServerFileInfo>> GetFilesAsync(string relativePath = null);
        Task<bool> UploadFileChunkedAsync(string baseUrl, string filePath, int chunkSize = 2 * 1024 * 1024, IProgress<double> progress = null);
    }
}
