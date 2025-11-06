using System.Net.Http.Headers;
using System.Text.Json;
using PowerLab.UpdaterService.Contracts;
using PowerLab.UpdaterService.Models;
using PowerLab.UpdaterService.Tools;
using Serilog;

namespace PowerLab.UpdaterService.Services
{
    public class FileRespository : IFileRespository
    {
        private readonly HttpClient _client;
        private readonly string _baseApiUrl;
        public FileRespository(IConfiguration config)
        {
            _baseApiUrl = config["ServerPoint"] ?? throw new Exception("未找到 ServerPoint 配置");
            _client = new HttpClient();
        }

        public async Task<bool> DownloadFileAsync(string url, string outputPath, IProgress<double> progress)
        {
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[81920];
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
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<List<ServerFileInfo>> GetFilesAsync(string relativePath = null)
        {
            HttpHelper httpHelper = new HttpHelper();
            var result = await httpHelper.GetAsync<List<ServerFileInfo>>($"{_baseApiUrl}/api/file/list");
            return result;
        }


        // ⚡ 断点续传上传
        public async Task<bool> UploadFileChunkedAsync(string baseUrl, string filePath, int chunkSize = 2 * 1024 * 1024, IProgress<double> progress = null)
        {
            var fileInfo = new FileInfo(filePath);
            long totalSize = fileInfo.Length;
            int totalChunks = (int)Math.Ceiling(totalSize / (double)chunkSize);
            string fileName = fileInfo.Name;

            // 1️⃣ 获取已上传的分块信息
            var uploadedChunks = await GetUploadedChunksAsync(baseUrl, fileName);

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[chunkSize];
            long uploadedBytes = uploadedChunks.Count * (long)chunkSize;

            for (int chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++)
            {
                if (uploadedChunks.Contains(chunkIndex))
                {
                    // 跳过已上传分块
                    fs.Seek(chunkSize, SeekOrigin.Current);
                    continue;
                }

                int bytesRead = await fs.ReadAsync(buffer);
                using var content = new MultipartFormDataContent();
                var byteContent = new ByteArrayContent(buffer, 0, bytesRead);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                content.Add(byteContent, "file", fileName);
                content.Add(new StringContent(fileName), "fileName");
                content.Add(new StringContent(chunkIndex.ToString()), "chunkIndex");
                content.Add(new StringContent(totalChunks.ToString()), "totalChunks");

                // 2️⃣ 上传当前分块
                var response = await _client.PostAsync($"{baseUrl}/upload", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"分块 {chunkIndex} 上传失败: {response.StatusCode}");
                    return false;
                }

                uploadedBytes += bytesRead;
                double percent = uploadedBytes * 100.0 / totalSize;
                progress?.Report(percent);
            }

            Console.WriteLine("✅ 上传完成");
            return true;
        }

        // 🧩 调用 /api/file/progress，获取已上传分块索引列表
        private async Task<HashSet<int>> GetUploadedChunksAsync(string baseUrl, string fileName)
        {
            try
            {
                var response = await _client.GetAsync($"{baseUrl}/progress?fileName={Uri.EscapeDataString(fileName)}");
                if (!response.IsSuccessStatusCode)
                    return [];

                var json = await response.Content.ReadAsStringAsync();
                var uploadedChunks = JsonSerializer.Deserialize<HashSet<int>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

                Console.WriteLine($"已上传分块: {string.Join(",", uploadedChunks)}");
                return uploadedChunks;
            }
            catch
            {
                return [];
            }
        }
    }
}
