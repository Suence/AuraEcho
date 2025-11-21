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
        private readonly string _baseApiUrl;
        public FileRespository(IConfiguration config)
        {
            _baseApiUrl = config["ServerPoint"] ?? throw new Exception("未找到 ServerPoint 配置");
        }

        public async Task<bool> DownloadFileAsync(string fileId, string outputPath, IProgress<double> progress)
        {
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync($"{_baseApiUrl}/api/file/download?fileId={fileId}", HttpCompletionOption.ResponseHeadersRead);
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

    }
}
