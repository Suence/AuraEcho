using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;

namespace PowerLab.Core.Repositories;

public class FileRespository : IFileRespository
{
    private readonly HttpClient _client;

    public FileRespository()
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

    public async Task<List<UploadedFile>> GetUploadedFilesAsync()
    {
        HttpHelper httpHelper = new HttpHelper();
        var result = await httpHelper.GetAsync<List<UploadedFile>>($"{Urls.ServerUrl}/api/file/UploadFileList");
        return result;
    }

    public async Task<string> UploadFileAsync(string filePath, string type)
    {
        using var form = new MultipartFormDataContent();
        using var fs = File.OpenRead(filePath);
        var streamContent = new StreamContent(fs);
        form.Add(streamContent, "file", Path.GetFileName(filePath));
        form.Add(new StringContent(type), "type");
        var resp = await _client.PostAsync($"{Urls.ServerUrl}/api/file/upload", form);
        resp.EnsureSuccessStatusCode();
        var result = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("fileId").ToString();
    }

    public async Task<string> UploadWithChunksAsync(string filePath, string fileType, IProgress<double> progress)
    {
        var fi = new FileInfo(filePath);
        int chunkSize = 2 * 1024 * 1024;
        int totalChunks = (int)Math.Ceiling((double)fi.Length / chunkSize);

        using var client = new HttpClient();
        // init
        var initForm = new MultipartFormDataContent
            {
                { new StringContent(fi.Name), "fileName" },
                { new StringContent(fileType), "fileType" },
                { new StringContent(fi.Length.ToString()), "totalSize" },
                { new StringContent(totalChunks.ToString()), "totalChunks" }
            };
        var initResp = await client.PostAsync($"{Urls.ServerUrl}/api/file/uploadInit", initForm);
        initResp.EnsureSuccessStatusCode();
        var initJson = await initResp.Content.ReadFromJsonAsync<JsonElement>();
        var uploadId = initJson.GetProperty("uploadId").GetString()!;

        // try get already uploaded chunks (in case resume)
        var uploadedResp = await client.GetAsync($"{Urls.ServerUrl}/api/file/uploadedChunks?uploadId={uploadId}");
        var uploadedArr = await uploadedResp.Content.ReadFromJsonAsync<int[]>() ?? Array.Empty<int>();
        var uploadedSet = new HashSet<int>(uploadedArr);

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
            var chunkResp = await client.PostAsync($"{Urls.ServerUrl}/api/file/uploadchunk", content);
            chunkResp.EnsureSuccessStatusCode();

            uploadedBytes += read;
            progress?.Report(uploadedBytes * 100D / fi.Length);
        }

        // merge
        var mergeForm = new MultipartFormDataContent { { new StringContent(uploadId), "uploadId" } };
        var mergeResp = await client.PostAsync($"{Urls.ServerUrl}/api/file/uploadMerge", mergeForm);
        mergeResp.EnsureSuccessStatusCode();
        var mergeJson = await mergeResp.Content.ReadFromJsonAsync<JsonElement>();
        return mergeJson.GetProperty("fileId").GetString()!;
    }

}
