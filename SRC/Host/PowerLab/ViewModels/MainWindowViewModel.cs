using System.ComponentModel;
using PowerLab.Constants;
using PowerLab.Core.Events;
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
    #endregion

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

    public MainWindowViewModel(INavigationService navigationService, IEventAggregator eventAggregator)
    {
        NavigationService = navigationService;
        _eventAggregator = eventAggregator;

        GoBackCommand = new DelegateCommand(GoBack, CanGoBack);

        _eventAggregator.GetEvent<RequestViewEvent>().Subscribe(GoToTargetView);
        _eventAggregator.GetEvent<SignInExpiredEvent>().Subscribe(SignInExpired);

        if (NavigationService is INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NavigationService.CanGoBack))
                    GoBackCommand.RaiseCanExecuteChanged();
            };
        }
    }
}
