using System.ComponentModel;
using System.Threading.Tasks;
using PowerLab.Constants;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Events;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;
using PowerLab.Core.Tools;
using PowerLab.PluginContracts.Constants;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace PowerLab.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region private members
    private string _title = "PowerLab";
    private readonly IAuthRepository _authRepository;
    private readonly IClientSession _clientSession;
    #endregion

    private Task _autoSignInTask;
    public INavigationService NavigationService
    {
        get;
        private set => SetProperty(ref field, value);
    }
    private readonly IEventAggregator _eventAggregator;

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    public DelegateCommand GoBackCommand { get; }
    public bool CanGoBack() => NavigationService.CanGoBack;
    private void GoBack()
    {
        NavigationService.GoBack();
    }

    private void SignInExpired()
    {
        NavigationService.RequestNavigate(HostRegionNames.ContentDialogRegion, ViewNames.SignInExpired);
    }
    private void GoToTargetView(string viewName)
    {
        NavigationService.RequestNavigate(HostRegionNames.MainRegion, viewName);
    }

    public DelegateCommand AutoSignInCommand { get; }
    private async void AutoSignIn()
    {
        await _autoSignInTask;
        if (_clientSession.IsSignedIn)
        {
            NavigationService.RequestNavigate(HostRegionNames.HomeRegion, ViewNames.Homepage, canBack: false);
            return;
        }
        NavigationService.RequestNavigate(HostRegionNames.HomeRegion, ViewNames.SignIn, canBack: false);
    }

    public MainWindowViewModel(INavigationService navigationService, IEventAggregator eventAggregator, IAuthRepository authRepository, IClientSession clientSession)
    {
        NavigationService = navigationService;
        _eventAggregator = eventAggregator;
        _authRepository = authRepository;
        _clientSession = clientSession;

        GoBackCommand = new DelegateCommand(GoBack, CanGoBack);

        _eventAggregator.GetEvent<RequestViewEvent>().Subscribe(GoToTargetView);
        _eventAggregator.GetEvent<SignInExpiredEvent>().Subscribe(SignInExpired);
        AutoSignInCommand = new DelegateCommand(AutoSignIn);

        if (NavigationService is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NavigationService.CanGoBack))
                    GoBackCommand.RaiseCanExecuteChanged();
            };
        }

        _autoSignInTask = AutoSignInAsync();
    }

    private async Task AutoSignInAsync()
    {
        var refreshToken = SecureStore.Load(SecureStoreKeys.RefreshToken);
        if (refreshToken is null) return;

        var result = await _authRepository.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        });

        if (result is null)
        {
            SecureStore.Delete(SecureStoreKeys.RefreshToken);
            return;
        }

        _clientSession.SignIn(new AppToken
        { 
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt
        });
    }
}
