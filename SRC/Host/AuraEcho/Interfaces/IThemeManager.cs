using AuraEcho.PluginContracts.Interfaces;
using AuraEcho.PluginContracts.Models;
using System.Collections.Generic;

namespace AuraEcho.Interfaces;

public interface IThemeManager
{
    /// <summary>
    /// 当前主题
    /// </summary>
    AppTheme CurrentTheme { get; set; }

    void ApplyTheme(AppTheme appTheme);

    void AttachPluginTheme(IPlugin plugin);

    void AttachPluginThemes(IEnumerable<IPlugin> plugins);
}
