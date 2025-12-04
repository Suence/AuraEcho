namespace PowerLab.Core.Contracts
{
    public interface IPluginInstallService
    {
        public Task<bool> InstallAsync(string filePath);
    }
}
