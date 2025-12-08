using System.IO;
using System.Net.Http;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class FileRepository : IFileRepository
{
    private readonly HttpClient _client;

    public FileRepository()
    {
        _client = new HttpClient();
    }

    public async Task<bool> DownloadFileAsync(string fileId, string outputPath, IProgress<double> progress)
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync($"{Urls.ServerUrl}/api/file/download?fileId={fileId}", HttpCompletionOption.ResponseHeadersRead);
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
                    progress?.Report(percent);
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    public async Task<UploadedFile> GetFileByIdAsync(string fileId)
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<UploadedFile>($"{Urls.ServerUrl}/api/file/{fileId}");
        return result;
    }
    public async Task<List<UploadedFile>> GetUploadedFilesAsync()
    {
        HttpHelper httpHelper = new HttpHelper();
        var response = await httpHelper.GetAsync<UploadFileListResponse>($"{Urls.ServerUrl}/api/file/UploadFileList");
        if (response is null) return null;

        var result = response.Files.Select(f => new UploadedFile
        {
            FileName = f.FileName,
            Id = f.Id,
            MimeType = f.Type,
            RelativePath = f.RelativePath,
            Size = f.Size,
            UploadTime = f.UploadTime,
        }).ToList();
        return result;

    }

    public async Task<string> UploadFileAsync(string filePath, string type)
    {
        using var form = new MultipartFormDataContent();
        using var fs = File.OpenRead(filePath);
        var streamContent = new StreamContent(fs);
        form.Add(streamContent, "file", Path.GetFileName(filePath));
        form.Add(new StringContent(type), "type");
        var helper = new HttpHelper();
        var response = await helper.PostAsync<UploadFileResponse>($"{Urls.ServerUrl}/api/file/upload", form);
        if (response is null) return null;

        return response.FileId;
    }

    public async Task<string> UploadWithChunksAsync(string filePath, string fileType, IProgress<double> progress)
    {
        var fi = new FileInfo(filePath);
        int chunkSize = 2 * 1024 * 1024;
        int totalChunks = (int)Math.Ceiling((double)fi.Length / chunkSize);

        var helper = new HttpHelper();
        // init
        var initForm = new MultipartFormDataContent
        {
            { new StringContent(fi.Name), "fileName" },
            { new StringContent(fileType), "fileType" },
            { new StringContent(fi.Length.ToString()), "totalSize" },
            { new StringContent(totalChunks.ToString()), "totalChunks" }
        };

        var initResp = await helper.PostAsync<UploadInitResponse>($"{Urls.ServerUrl}/api/file/uploadinit", initForm);
        if (initResp is null) return null;

        var uploadId = initResp.UploadId;

        // try get already uploaded chunks (in case resume)
        var uploadedResp = await helper.GetAsync<UploadedChunksResponse>($"{Urls.ServerUrl}/api/file/uploadedChunks?uploadId={uploadId}");
        var uploadedSet = new HashSet<int>(uploadedResp?.ChunkParts ?? []);

        long uploadedBytes = (long)uploadedSet.Count * chunkSize;
        progress.Report((double)uploadedBytes / fi.Length);

        using var fs = File.OpenRead(filePath);
        byte[] buffer = new byte[chunkSize];
        for (int i = 0; i < totalChunks; i++)
        {
            if (uploadedSet.Contains(i)) { fs.Position += chunkSize; continue; }
            int read = await fs.ReadAsync(buffer.AsMemory());
            if (read <= 0) break;
            using var content = new MultipartFormDataContent
            {
                { new StringContent(uploadId), "uploadId" },
                { new StringContent(i.ToString()), "chunkIndex" },
                { new StreamContent(new MemoryStream(buffer, 0, read)), "chunk", $"chunk{i}" }
            };
            var uploadResult = await helper.PostAsync($"{Urls.ServerUrl}/api/file/uploadchunk", content);
            if (!uploadResult) return null;

            uploadedBytes += read;
            progress?.Report(uploadedBytes * 100D / fi.Length);
        }

        // merge
        var mergeForm = new MultipartFormDataContent { { new StringContent(uploadId), "uploadId" } };

        var mergeResp = await helper.PostAsync<UploadMergeResponse>($"{Urls.ServerUrl}/api/file/uploadMerge", mergeForm);
        if (mergeResp is null) return null;

        return mergeResp.FileId;
    }

}
