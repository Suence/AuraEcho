using System;
using System.Globalization;
using PowerLab.Core.Tools;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Events;
using PowerLab.PluginContracts.Models;
using Prism.Events;
using Prism.Mvvm;

namespace PowerLab.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        #region private members
        private readonly IEventAggregator _eventAggregator;
        private readonly IThemeManager _themeManager;
        private AppLanguage _appLanguage;
        private AppTheme _appTheme;
        #endregion

        public AppLanguage AppLanguage
        {
            get => _appLanguage;
            set
            {
                if (SetProperty(ref _appLanguage, value))
                {
                    LanguageChanged(value);
                }
            }
        }

        private void LanguageChanged(AppLanguage language)
        {
            var targetCultureInfo = language switch
            {
                AppLanguage.ChineseSimplified => new CultureInfo("zh-CN"),
                AppLanguage.English => new CultureInfo("en-US"),
                _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
            };

            ApplicationResources.ChangeCulture(targetCultureInfo);

            _eventAggregator.GetEvent<AppLanguageChangedEvent>().Publish(language);
        }

        public AppTheme AppTheme
        {
            get => _appTheme;
            set
            {
                bool isUpadted = SetProperty(ref _appTheme, value);
                if (isUpadted)
                {
                    ApplyTheme();
                }
            }
        }

        private void ApplyTheme()
        {
            _themeManager.CurrentTheme = AppTheme;
        }

        public SettingsViewModel(IEventAggregator eventAggregator, IThemeManager themeManager)
        {
            _eventAggregator = eventAggregator;
            _themeManager = themeManager;
        }
    }
}
