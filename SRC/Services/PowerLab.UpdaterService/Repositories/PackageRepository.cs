using PowerLab.UpdaterService.Constants;
using PowerLab.UpdaterService.Contracts;
using PowerLab.UpdaterService.Models;
using PowerLab.UpdaterService.Tools;

namespace PowerLab.UpdaterService.Services
{
    public class PackageRepository : IPackageRepository
    {
        public async Task<bool> DownloadLatestAsync(string build, string outputPath, IProgress<double> progress)
        {
            try
            {
                using var client = new HttpClient();
                using var response = await client.GetAsync($"{Urls.ServerUrl}/api/package/download?build={build}", HttpCompletionOption.ResponseHeadersRead);
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

        public async Task<AppVersionInfo> GetLatestAsync()
        {
            HttpHelper httpHelper = new HttpHelper();
            var result = await httpHelper.GetAsync<AppVersionInfo>($"{Urls.ServerUrl}/api/package/latest");
            return result;
        }

    }
}
