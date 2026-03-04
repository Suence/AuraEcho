using Prism.Ioc;

namespace AuraEcho.PluginContracts.Interfaces;

public interface IPluginSetup
{
    void Setup(IContainerProvider containerProvider);
}
