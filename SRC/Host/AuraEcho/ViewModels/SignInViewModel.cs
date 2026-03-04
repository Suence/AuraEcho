using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using AuraEcho.Core.Contracts;
using AuraEcho.Core.Models;
using AuraEcho.Core.Models.Api;
using AuraEcho.PluginContracts.Constants;
using AuraEcho.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace AuraEcho.ViewModels;

public class SignInViewModel : BindableBase, IRegionMemberLifetime
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

    public DelegateCommand SignInCommand { get; }
    private bool CanSignIn()
        => !String.IsNullOrWhiteSpace(UserName) &&
           !String.IsNullOrWhiteSpace(Password);

    public DelegateCommand NavigationToSignUpCommand { get; }

    public bool KeepAlive => false;

    private void NavigationToSignUp()
    {
        _regionManager.RequestNavigate(HostRegionNames.HomeRegion, nameof(SignUp));
    }

    private async void SignIn()
    {
        SignInResponse? result = await _authRepository.SignInAsync(new SignInRequest
        {
            UserName = UserName.Trim(),
            Password = Password.Trim()
        });

        if (result is null) return;

        _clientSession.SignIn(new AppToken
        { 
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresAt = result.ExpiresAt
        });
        _regionManager.RequestNavigate(HostRegionNames.HomeRegion, nameof(Homepage));
    }

    public SignInViewModel(IRegionManager regionManager, IAuthRepository authRepository, IClientSession clientSession)
    {
        _regionManager = regionManager;
        _authRepository = authRepository;
        _clientSession = clientSession;

        SignInCommand = new DelegateCommand(SignIn, CanSignIn)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password);

        NavigationToSignUpCommand = new DelegateCommand(NavigationToSignUp);
        _clientSession = clientSession;
    }
}
