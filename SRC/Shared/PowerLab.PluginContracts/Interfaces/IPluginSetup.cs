using Prism.Ioc;

namespace PowerLab.PluginContracts.Interfaces
{
    public interface IPluginSetup
    {
        void Setup(IContainerProvider containerProvider);
    }
}
