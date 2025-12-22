using System;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Models.Api;
using PowerLab.PluginContracts.Constants;
using PowerLab.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels;

public class SignUpViewModel : BindableBase, IRegionMemberLifetime
{
    private readonly IRegionManager _regionManager;
    private readonly IAuthRepository _authRepository;
    private readonly IClientSession _clientSession;

    public string UserName
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Password
    {
        get;
        set => SetProperty(ref field, value);
    }

    public DelegateCommand SignUpCommand { get; }
    private bool CanSignUp()
        => !String.IsNullOrWhiteSpace(UserName) &&
           !String.IsNullOrWhiteSpace(Password);

    public DelegateCommand NavigationToSignInCommand { get; }

    public bool KeepAlive => false;

    private void NavigationToSignIn()
    {
        _regionManager.RequestNavigate(HostRegionNames.HomeRegion, nameof(SignIn));
    }

    private async void SignUp()
    {
        var result = await _authRepository.SignUpAsync(new SignUpRequest
        {
            UserName = UserName.Trim(),
            Password = Password.Trim()
        });

        if (result is null) return;

        _clientSession.SignIn(result.AccessToken, new UserProfile
        {
            Id = result.User.UserId,
            UserName = result.User.UserName
        });
        _regionManager.RequestNavigate(HostRegionNames.HomeRegion, nameof(Homepage));
    }

    public SignUpViewModel(IRegionManager regionManager, IAuthRepository authRepository, IClientSession clientSession)
    {
        _regionManager = regionManager;
        _authRepository = authRepository;
        _clientSession = clientSession;

        SignUpCommand = new DelegateCommand(SignUp, CanSignUp)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password);

        NavigationToSignInCommand = new DelegateCommand(NavigationToSignIn);
    }
}
