using Prism.Regions;

namespace AuraEcho.Installer.Bootstrapper.Extensions;

public static class IRegionManagerExtensions
{
    public static void RequestNavigateOnUIThread(this IRegionManager @this, string regionName, string viewName)
        => App.Current.Dispatcher.Invoke(() => @this.RequestNavigate(regionName, viewName));
}
