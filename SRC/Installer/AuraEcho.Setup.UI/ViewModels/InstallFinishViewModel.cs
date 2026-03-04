using AuraEcho.Installer.Bootstrapper.WixToolset;
using Prism.Commands;
using Prism.Mvvm;

namespace AuraEcho.Installer.Bootstrapper.ViewModels;

public class InstallFinishViewModel : BindableBase
{
    private readonly AuraEchoBootstrapper _ba;

    public DelegateCommand FinishedCommand { get; }
    private async void Finished()
    {
        _ba.LaunchExecutedExe(_ba.AppLauncherFullName, null!);
        App.Current.Shutdown();
    }

    public InstallFinishViewModel(AuraEchoBootstrapper ba)
    {
        _ba = ba;
        FinishedCommand = new DelegateCommand(Finished);
    }
}
