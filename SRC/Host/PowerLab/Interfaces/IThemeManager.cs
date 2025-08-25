using System;
using System.Collections;
using System.Collections.Generic;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;

namespace PowerLab.Interfaces
{
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
}
