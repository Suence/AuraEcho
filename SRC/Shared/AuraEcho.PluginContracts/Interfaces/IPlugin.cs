using Prism.Modularity;

namespace AuraEcho.PluginContracts.Interfaces;

public interface IPlugin : IModule, IPluginTheme, IPluginSettings, IPluginSetup
{
}
