using PowerLab.ExternalTools.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PowerLab.ExternalTools
{
    public class ExternalToolsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ExternalToolsHome>();
            containerRegistry.RegisterForNavigation<AddExternalTool>();
            containerRegistry.RegisterForNavigation<EditExternalTool>();
        }
    }
}