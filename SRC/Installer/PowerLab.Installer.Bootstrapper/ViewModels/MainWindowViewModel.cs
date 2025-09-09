using System.Windows;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.Extensions;
using PowerLab.Installer.Bootstrapper.WixToolset;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly PowerLabBootstrapper _ba;

        public DelegateCommand DetectPackageCommand { get; }
        private void DetectPackage()
        {
            string targetView =
                _ba.Command.Action == LaunchAction.Uninstall
                ? InstallerViewNames.UninstallPreparation
                : InstallerViewNames.InstallPreparation;

            _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, targetView);
        }

        public DelegateCommand ExitCommand { get; }
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"></param>
        public MainWindowViewModel(PowerLabBootstrapper ba, IRegionManager regionManager)
        {
            _ba = ba;
            _regionManager = regionManager;
            DetectPackageCommand = new DelegateCommand(DetectPackage);
            ExitCommand = new DelegateCommand(Exit);
        }
    }
}
