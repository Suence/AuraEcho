using PowerLab.UpdaterService.Models;

namespace PowerLab.UpdaterService.Contracts
{
    public interface IVersionRespository
    {
        Task<AppVersionInfo> GetLatestAsync();
    }
}
