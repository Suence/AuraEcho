using System;
using System.Windows;
using PowerLab.FishyTime.Themes;
using PowerLab.FishyTime.Views;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using Prism.Ioc;
using Prism.Modularity;

namespace PowerLab.FishyTime
{
    public class FishyTimeModule : IPlugin
    {
        private readonly ResourceDictionary _lightTheme = new FishyTimeLightTheme();
        private readonly ResourceDictionary _darkTheme = new FishyTimeDarkTheme();

        public AppSettingsItem GetSettings()
        {
            return new AppSettingsItem
            {
                Name = "FISHYTIME",
                ViewName = nameof(FishyTimeSettings)
            };
        }

        public ResourceDictionary GetThemeResource(AppTheme theme)
        {
            return theme switch
            {
                AppTheme.Light => _lightTheme,
                AppTheme.Dark => _darkTheme,
                _ => null
            };
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FishyTimeHome>();
            containerRegistry.RegisterForNavigation<FishyTimeSettings>();
        }

        public void Setup(IContainerProvider containerProvider)
        {
        }
    }
}