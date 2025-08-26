using PowerLab.PluginContracts.Models;

namespace PowerLab.Core.Models
{
    public class HostSettings
    {
        public AppTheme AppTheme { get; set; }
        public AppLanguage AppLanguage { get; set; }
        public bool HardwareAcceleration { get; set; }
        public static HostSettings Default => new()
        {
            AppTheme = AppTheme.FollowSystem,
            AppLanguage = AppLanguage.ChineseSimplified,
            HardwareAcceleration = true
        };
    }
}
