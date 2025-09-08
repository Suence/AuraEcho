using System.Diagnostics;
using System.IO;
using Prism.Commands;
using Prism.Mvvm;

namespace PowerLab.Installer.Bootstrapper.ViewModels
{
    public class InstallFinishViewModel : BindableBase
    {
        private readonly PowerLabBootstrapper _ba;

        private bool _isRunWhenExited = true;
        public bool IsRunWhenExited
        {
            get => _isRunWhenExited;
            set => SetProperty(ref _isRunWhenExited, value);
        }

        public DelegateCommand FinishedCommand { get; }
        private async void Finished()
        {
            if (IsRunWhenExited)
            {
                string appFilePath = Path.Combine(_ba.Engine.GetVariableString("InstallFolder"), "PowerLab.exe");
                await Task.Run(() => Process.Start(appFilePath));
            }
            App.Current.Shutdown();
        }

        public InstallFinishViewModel(PowerLabBootstrapper ba)
        {
            _ba = ba;
            IsRunWhenExited = true;
            FinishedCommand = new DelegateCommand(Finished);
        }
    }
}
