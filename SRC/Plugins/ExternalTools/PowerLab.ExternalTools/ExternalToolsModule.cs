using System.Windows;
using PowerLab.ExternalTools.Themes;
using PowerLab.ExternalTools.Views;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.PluginContracts.Models;
using Prism.Ioc;

namespace PowerLab.ExternalTools
{
    public class ExternalToolsModule : IPlugin
    {
        private readonly ResourceDictionary _lightTheme = new ExternalToolsLightTheme();
        private readonly ResourceDictionary _darkTheme = new ExternalToolsDarkTheme();
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
            containerRegistry.RegisterForNavigation<ExternalToolsHome>();
            containerRegistry.RegisterForNavigation<AddExternalTool>();
            containerRegistry.RegisterForNavigation<EditExternalTool>();
        }
    }
}