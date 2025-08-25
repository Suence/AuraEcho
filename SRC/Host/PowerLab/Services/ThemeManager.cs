using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using PowerLab.Core.Contracts;
using PowerLab.Core.Extensions;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using PowerLab.Themes;
using Prism.Mvvm;

namespace PowerLab.Services
{
    public class ThemeManager : BindableBase, IThemeManager
    {
        private readonly ILogger _logger;
        private readonly IPluginManager _pluginManager;
        private AppTheme _currentTheme;
        private readonly List<ResourceDictionary> _themeResources = [];
        private bool _isInitialized;

        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                bool isUpdated = SetProperty(ref _currentTheme, value);

                if (isUpdated || !_isInitialized)
                {
                    ApplyTheme(value);
                }
                _isInitialized = true;
            }
        }

        public ThemeManager(ILogger logger, IPluginManager pluginManager)
        {
            _logger = logger;
            _pluginManager = pluginManager;

            SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
        }

        public void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category != UserPreferenceCategory.General) return;

            if (CurrentTheme != AppTheme.FollowSystem) return;

            var systemTheme = GetSystemTheme();
            ApplyTheme(systemTheme);
        }

        public void ApplyTheme(AppTheme appTheme)
        {
            AppTheme realTheme = appTheme == AppTheme.FollowSystem ? GetSystemTheme() : appTheme;
            try
            {
                ResourceDictionary hostThemeResources = GetHostThemeResource(realTheme);
                List<ResourceDictionary> pluginThemeResources = GetPluginThemeResources(realTheme);

                _themeResources.Add(hostThemeResources);
                _themeResources.AddRange(pluginThemeResources);

                ClearTheme();
                Application.Current.Resources.MergedDictionaries.Add(hostThemeResources);
                pluginThemeResources.ForEach(Application.Current.Resources.MergedDictionaries.Add);

                _logger.Debug($"主题切换成功：{realTheme} (Host + {_themeResources.Count} 插件资源)");
            }
            catch (Exception ex)
            {
                _logger.Error($"切换主题失败：{realTheme}，异常：{ex.Message}");
            }
        }

        private List<ResourceDictionary> GetPluginThemeResources(AppTheme appTheme)
        {
            var resources = new List<ResourceDictionary>();
            foreach (var registry in _pluginManager.Plugins)
            {
                if (registry.PluginContext is not IPlugin plugin) continue;
                var pluginResource = plugin.GetThemeResource(appTheme);
                if (pluginResource != null)
                {
                    resources.Add(pluginResource);
                }
            }
            return resources;
        }

        private static ResourceDictionary GetHostThemeResource(AppTheme appTheme)
        {
            return appTheme switch
            {
                AppTheme.Light => LightTheme.Instance,
                AppTheme.Dark => DarkTheme.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(appTheme), appTheme, null)
            };
        }

        public void ClearTheme()
        {
            foreach (var dict in _themeResources)
                Application.Current.Resources.MergedDictionaries.Remove(dict);

            _themeResources.Clear();
        }

        public static AppTheme GetSystemTheme()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key is null) return AppTheme.Light;

            int appsUseLightTheme = (int)key.GetValue("AppsUseLightTheme", -1);
            key.Close();

            // 0: 暗色 1：亮色
            if (appsUseLightTheme == 0)
            {
                return AppTheme.Dark;
            }
            return AppTheme.Light;
        }

        public void AttachPluginTheme(IPlugin plugin)
        {
            AppTheme realTheme = CurrentTheme == AppTheme.FollowSystem ? GetSystemTheme() : CurrentTheme;
            var pluginThemeResource = plugin.GetThemeResource(realTheme);
            Application.Current.Resources.MergedDictionaries.Add(pluginThemeResource);
        }

        public void AttachPluginThemes(IEnumerable<IPlugin> plugins)
        {
            plugins.ForEach(AttachPluginTheme);
        }
    }

}
