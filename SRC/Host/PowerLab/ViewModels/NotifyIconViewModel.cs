using PowerLab.Core.Events;
using Prism.Commands;
using Prism.DryIoc;
using Prism.Events;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PowerLab.ViewModels
{
    public class NotifyIconViewModel
    {
        #region private
        private IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;
        #endregion

        public ICommand ShowWindowCommand { get; }
        private void ShowWindow()
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                Application.Current.MainWindow.WindowState = WindowState.Normal;

            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Activate();
            Application.Current.MainWindow.Topmost = true;
            Application.Current.MainWindow.Topmost = false;
            Application.Current.MainWindow.Focus();
        }

        public ICommand ExitApplicationCommand { get; }
        private void ExitApplication() => Application.Current.Shutdown();


        public DelegateCommand<string> GoToTargetViewCommand { get; }
        private async void GoToTargetView(string viewName)
        {
            _eventAggregator.GetEvent<RequestViewEvent>().Publish(viewName);
            await Task.Delay(100);
            ShowWindow();
        }

        public NotifyIconViewModel()
        {
            ExitApplicationCommand = new DelegateCommand(ExitApplication);
            ShowWindowCommand = new DelegateCommand(ShowWindow);
            GoToTargetViewCommand = new DelegateCommand<string>(GoToTargetView);
                 
            var container = (Application.Current as PrismApplication).Container;
            _eventAggregator = container.Resolve(typeof(IEventAggregator)) as IEventAggregator;
            _regionManager = container.Resolve(typeof(IRegionManager)) as IRegionManager;
        }
    }

    //public class DelegateCommand : ICommand
    //{
    //    public DelegateCommand(Action commandAction, Func<bool> canExecuteFunc)
    //        => (CommandAction, CanExecuteFunc) = (commandAction, canExecuteFunc);

    //    public Action CommandAction { get; set; }
    //    public Func<bool> CanExecuteFunc { get; set; }

    //    public void Execute(object parameter)
    //        => CommandAction();

    //    public bool CanExecute(object parameter)
    //        => CanExecuteFunc?.Invoke() ?? true;

    //    public event EventHandler CanExecuteChanged
    //    {
    //        add => CommandManager.RequerySuggested += value; 
    //        remove => CommandManager.RequerySuggested -= value; 
    //    }
    //}
}
