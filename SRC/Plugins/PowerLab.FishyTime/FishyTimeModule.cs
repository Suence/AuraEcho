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
#if DEBUG
            containerProvider.Resolve<IRegionManager>()
                .RegisterViewWithRegion(HostRegionNames.PluginContentRegion, typeof(FishyTimeHome));
#endif
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}