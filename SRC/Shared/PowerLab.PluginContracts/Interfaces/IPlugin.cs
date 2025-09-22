using Prism.Modularity;

namespace PowerLab.PluginContracts.Interfaces;

public interface IPlugin : IModule, IPluginTheme, IPluginSettings, IPluginSetup
{
}
