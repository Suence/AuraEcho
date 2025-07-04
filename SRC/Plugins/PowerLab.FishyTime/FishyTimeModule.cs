using PowerLab.Core.Constants;
using PowerLab.FishyTime.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PowerLab.FishyTime
{
    public class FishyTimeModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FishyTimeHome>();
        }
    }
}