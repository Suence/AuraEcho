using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PowerLab.Core.Contracts;
using PowerLab.Core.Events;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels;

public class NotifyIconViewModel : BindableBase
{
    #region private
    private readonly IEventAggregator _eventAggregator;
    #endregion

    public bool IsSignedIn
    {
        get => field;
        set => SetProperty(ref field, value);
    } = false;

    public static bool ShutdownRequested { get; private set; }

    public ICommand ShowWindowCommand { get; }
    private void ShowWindow()
    {
        _eventAggregator.GetEvent<RequestShowAppEvent>().Publish();
    }

    public ICommand ExitApplicationCommand { get; }
    private void ExitApplication()
    {
        ShutdownRequested = true;
        Application.Current.Shutdown();
    }

    public DelegateCommand<string> GoToTargetViewCommand { get; }
    private void GoToTargetView(string viewName)
    {
        if (!IsSignedIn) return;

        _eventAggregator.GetEvent<RequestViewEvent>().Publish(viewName);
        ShowWindow();
    }

    public NotifyIconViewModel()
    {
        ExitApplicationCommand = new DelegateCommand(ExitApplication);
        ShowWindowCommand = new DelegateCommand(ShowWindow);
        GoToTargetViewCommand = new DelegateCommand<string>(GoToTargetView);
             
        var container = (Application.Current as PrismApplication)!.Container;
        _eventAggregator = (container.Resolve(typeof(IEventAggregator)) as IEventAggregator)!;
        _eventAggregator.GetEvent<SignedInEvent>().Subscribe(OnSignedIn, ThreadOption.UIThread);
        _eventAggregator.GetEvent<SignedOutEvent>().Subscribe(OnSignedOut, ThreadOption.UIThread);
    }

    private void OnSignedIn() => IsSignedIn = true;
    private void OnSignedOut() => IsSignedIn = false;
}
