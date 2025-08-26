using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Events;
using PowerLab.PluginContracts.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace PowerLab.ViewModels
{
    public class GeneralSettingsViewModel : BindableBase
    {
        #region private members
        private readonly IEventAggregator _eventAggregator;
        private readonly IThemeManager _themeManager;
        private readonly IHostSettingsProvider _hostSettingsProvider;
        private AppLanguage _appLanguage;
        private AppTheme _appTheme;
        private bool _hardwareAcceleration;
        #endregion

        public AppLanguage AppLanguage
        {
            get => _appLanguage;
            set
            {
                if (SetProperty(ref _appLanguage, value))
                {
                    LanguageChanged(value);
                    SaveSettings();
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
                    SaveSettings();
                }
            }
        }

        private void ApplyTheme()
        {
            _themeManager.CurrentTheme = AppTheme;
        }

        public bool HardwareAcceleration
        {
            get => _hardwareAcceleration;
            set
            {
                if (SetProperty(ref _hardwareAcceleration, value))
                {
                    HardwareAccelerationChanged(value);
                    SaveSettings();
                }
            }
        }

        private void HardwareAccelerationChanged(bool isEnabled)
        {
            RenderOptions.ProcessRenderMode = isEnabled ? RenderMode.Default : RenderMode.SoftwareOnly;
        }

        public DelegateCommand LoadSettingsCommand { get; }
        private void LoadSettings()
        {
            var settings = _hostSettingsProvider.LoadHostSettings();
            AppLanguage = settings.AppLanguage;
            AppTheme = settings.AppTheme;
            HardwareAcceleration = settings.HardwareAcceleration;
        }
        private void SaveSettings()
        {
            var settings = new HostSettings
            {
                AppLanguage = AppLanguage,
                AppTheme = AppTheme,
                HardwareAcceleration = HardwareAcceleration
            };
            _hostSettingsProvider.SaveHostSettings(settings);
        }


        public GeneralSettingsViewModel(IEventAggregator eventAggregator, IThemeManager themeManager, IHostSettingsProvider hostSettingsProvider)
        {
            _hostSettingsProvider = hostSettingsProvider;
            _eventAggregator = eventAggregator;
            _themeManager = themeManager;

            LoadSettingsCommand = new DelegateCommand(LoadSettings);
        }
    }
}
