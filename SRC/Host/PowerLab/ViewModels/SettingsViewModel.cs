using PowerLab.Constants;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;

namespace PowerLab.ViewModels;

public class SettingsViewModel : BindableBase
{
    #region private members
    private readonly IRegionManager _regionManager;
    private readonly IPluginManager _pluginManager;
    private ObservableCollection<AppSettingsItem> _settingsItems;
    #endregion

    public ObservableCollection<AppSettingsItem> SettingsItems
    {
        get => _settingsItems;
        set => SetProperty(ref _settingsItems, value);
    }

    public DelegateCommand LoadSettingsCommand { get; }
    private void LoadSettings()
    {
        SettingsItems =
        [
            new() 
            {
                Name = "General",
                ViewName = ViewNames.GeneralSettings
            }
        ];

        foreach (var plugin in _pluginManager.Plugins)
        {
            var pluginSettingsItem = plugin.PluginContext.GetSettings();
            if (SettingsItems.Contains(pluginSettingsItem)) continue;

            SettingsItems.Add(pluginSettingsItem);
        }

        NavigationToSettingsItem(ViewNames.GeneralSettings);
    }

    public DelegateCommand BackToHomeCommand { get; }
    private void BackToHome()
    {
        _regionManager.Regions[HostRegionNames.MainRegion].RemoveAll();
    }

    public DelegateCommand<string> NavigationToSettingsItemCommand { get; }
    private void NavigationToSettingsItem(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName)) return;

        _regionManager.RequestNavigate(HostRegionNames.SettingsContentRegion, viewName);
    }

    public SettingsViewModel(IRegionManager regionManager, IPluginManager pluginManager)
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));

        NavigationToSettingsItemCommand = new DelegateCommand<string>(NavigationToSettingsItem);
        LoadSettingsCommand = new DelegateCommand(LoadSettings);
        BackToHomeCommand = new DelegateCommand(BackToHome);
    }
}
