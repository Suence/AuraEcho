using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IFileRepository
{
    Task<List<UploadedFile>> GetUploadedFilesAsync();
    Task<string> UploadFileAsync(string filePath, string type);
    Task<string> UploadWithChunksAsync(string filePath, string fileType, IProgress<double> progress);
    Task<bool> DownloadFileAsync(string fileId, string outputPath, IProgress<double> progress);
}