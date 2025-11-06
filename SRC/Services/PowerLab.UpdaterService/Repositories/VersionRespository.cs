using PowerLab.UpdaterService.Contracts;
using PowerLab.UpdaterService.Models;
using PowerLab.UpdaterService.Tools;

namespace PowerLab.UpdaterService.Services
{
    public class VersionRespository : IVersionRespository
    {
        private readonly string _baseApiUrl;

        public VersionRespository(IConfiguration config)
        {
            _baseApiUrl = config["ServerPoint"] ?? throw new Exception("未找到 ServerPoint 配置");
        }

        public async Task<AppVersionInfo> GetLatestAsync()
        {
            HttpHelper httpHelper = new HttpHelper();
            var result = await httpHelper.GetAsync<AppVersionInfo>($"{_baseApiUrl}/api/version/latest");
            return result;
        }
    }
}
