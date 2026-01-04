using PowerLab.Core.Models;

namespace PowerLab.Core.Contracts;

public interface IFileRepository
{
    Task<List<UploadedFile>> GetUploadedFilesAsync();
    Task<Guid?> UploadFileAsync(string filePath, string type);
    Task<Guid?> UploadWithChunksAsync(string filePath, string fileType, IProgress<double> progress);
    Task<bool> DownloadFileAsync(Guid fileId, string outputPath, IProgress<double> progress);
    Task<UploadedFile> GetFileByIdAsync(Guid fileId);
}