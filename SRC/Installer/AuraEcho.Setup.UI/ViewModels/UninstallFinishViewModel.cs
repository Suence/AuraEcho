using Prism.Commands;
using Prism.Mvvm;

namespace AuraEcho.Installer.Bootstrapper.ViewModels;

public class UninstallFinishViewModel : BindableBase
{
    public DelegateCommand FinishedCommand { get; }
    private void Finished()
    {
        App.Current.Shutdown();
    }

    public UninstallFinishViewModel()
    {
        FinishedCommand = new DelegateCommand(Finished);
    }
}
