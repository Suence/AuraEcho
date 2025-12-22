using PowerLab.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Interfaces;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PowerLab.ViewModels;

public class SettingsViewModel : BindableBase
{
    #region private members
    private readonly IRegionManager _regionManager;
    private readonly IPluginManager _pluginManager;
    private readonly IAuthRepository _authRepository;
    private readonly IClientSession _clientSession;

    private ObservableCollection<AppSettingsItem> _settingsItems;
    #endregion

    public ObservableCollection<AppSettingsItem> SettingsItems
    {
        get => _settingsItems;
        set => SetProperty(ref _settingsItems, value);
    }

    public UserProfile CurrentUser
    {
        get;
        set => SetProperty(ref field, value);
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

    public DelegateCommand SignOutCommand { get; }
    private void SignOut()
    {
        _clientSession.SignOut();
        _regionManager.Regions[HostRegionNames.MainRegion].RemoveAll();
        _regionManager.RequestNavigate(HostRegionNames.HomeRegion, ViewNames.SignIn);
    }

    public SettingsViewModel(IRegionManager regionManager, IPluginManager pluginManager, IAuthRepository authRepository, IClientSession clientSession)
    {
        _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
        _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _clientSession = clientSession;

        NavigationToSettingsItemCommand = new DelegateCommand<string>(NavigationToSettingsItem);
        LoadSettingsCommand = new DelegateCommand(LoadSettings);
        BackToHomeCommand = new DelegateCommand(BackToHome);
        SignOutCommand = new DelegateCommand(SignOut);

        _ = LoadCurrentUserProfileAsync();
    }

    private async Task LoadCurrentUserProfileAsync()
    {
        var profile = await _authRepository.GetCurrentUserAsync();
        if (profile is null) return;

        CurrentUser = new UserProfile
        {
            Id = profile.UserId,
            UserName = profile.UserName
        };
    }
}
