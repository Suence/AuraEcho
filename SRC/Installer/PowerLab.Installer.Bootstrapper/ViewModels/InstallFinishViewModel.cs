using PowerLab.Installer.Bootstrapper.WixToolset;
using Prism.Commands;
using Prism.Mvvm;

namespace PowerLab.Installer.Bootstrapper.ViewModels;

public class InstallFinishViewModel : BindableBase
{
    private readonly PowerLabBootstrapper _ba;

    public DelegateCommand FinishedCommand { get; }
    private async void Finished()
    {
        _ba.Engine.LaunchApprovedExe(IntPtr.Zero, "LaunchMainApp", null);
        App.Current.Shutdown();
    }

    public InstallFinishViewModel(PowerLabBootstrapper ba)
    {
        _ba = ba;
        FinishedCommand = new DelegateCommand(Finished);
    }
}
