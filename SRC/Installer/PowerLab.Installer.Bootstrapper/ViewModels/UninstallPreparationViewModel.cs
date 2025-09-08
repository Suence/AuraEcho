using PowerLab.Installer.Bootstrapper.Constants;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.Installer.Bootstrapper.ViewModels
{
    public class UninstallPreparationViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand NavigationToUninstallCommand { get; }
        private void NavigationToUninstall()
        {
            _regionManager.RequestNavigate(InstallerRegionNames.MainRegion, InstallerViewNames.Uninstalling);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"></param>
        public UninstallPreparationViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigationToUninstallCommand = new DelegateCommand(NavigationToUninstall);
        }
    }
}
