using System;
using System.Globalization;
using PowerLab.Core.Tools;
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
        private AppLanguage _appLanguage;
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

        public SettingsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
    }
}
