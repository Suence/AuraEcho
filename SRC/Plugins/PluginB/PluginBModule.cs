using PluginB.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PluginB
{
    public class PluginBModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PluginBHomepage>();
        }
    }
}