using System.ComponentModel;
using PowerLab.PluginContracts.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace PowerLab.ViewModels;

public class MainWindowViewModel : BindableBase
{
    #region private members
    private string _title = "PowerLab";
    #endregion

    public INavigationService NavigationService
    {
        get => field;
        private set => SetProperty(ref field, value);
    }

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

    public MainWindowViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
        GoBackCommand = new DelegateCommand(GoBack, CanGoBack);

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
