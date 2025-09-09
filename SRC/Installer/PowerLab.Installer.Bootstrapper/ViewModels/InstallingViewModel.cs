using System.Diagnostics;
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
    public class InstallingViewModel : BindableBase, INavigationAware
    {
        private readonly PowerLabBootstrapper _ba;
        private readonly IRegionManager _regionManager;
        private string message;
        private int _progress;
        private bool _isCreateDesktopFolderShortcut;
        private bool _isRunAtBoot;
        #region Command
        /// <summary>
        /// 执行安装命令
        /// </summary>
        public DelegateCommand InstallCommand { get; }
        private void Install()
        {
            _ba.Install();
        }

        #endregion

        #region 属性

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }
        /// <summary>
        /// 安装总进度
        /// </summary>
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }


        #endregion 

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"></param>
        public InstallingViewModel(PowerLabBootstrapper ba, IRegionManager regionManager)
        {
            _ba = ba;
            _regionManager = regionManager;

            InstallCommand = new DelegateCommand(Install);
            SubscriptionInstallEvents();
        }

        #endregion



        #region 方法

        private void SubscriptionInstallEvents()
        {
            _ba.OnActionCompleted += InstallCompleted;
            _ba.OnPlanMsiFeature += PlanMsiFeature;
            _ba.OnProgress += UpdateProgress;
        }

        private void UpdateProgress(object? sender, int e)
        {
            Progress = e;
        }

        private void InstallCompleted(object? sender, EventArgs e)
        {
            _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, InstallerViewNames.InstallFinish);
        }

        private void UnsubscriptionInstallEvents()
        {
            _ba.OnActionCompleted -= InstallCompleted;
            _ba.OnPlanMsiFeature -= PlanMsiFeature;
            _ba.OnProgress -= UpdateProgress;
        }

        private void PlanMsiFeature(object sender, PlanMsiFeatureEventArgs e)
        {
            if (e.FeatureId == "DesktopFolderShortcutFeature")
            {
                e.State = _isCreateDesktopFolderShortcut ? FeatureState.Local : FeatureState.Absent;
                return;
            }
            if (e.FeatureId == "RunAtBootFeature")
            {
                e.State = _isRunAtBoot ? FeatureState.Local : FeatureState.Absent;
                return;
            }
            e.State = FeatureState.Local;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _isCreateDesktopFolderShortcut = (bool)navigationContext.Parameters["IsCreateDesktopFolderShortcut"];
            _isRunAtBoot = (bool)navigationContext.Parameters["IsRunAtBoot"];
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            UnsubscriptionInstallEvents();
        }

        #endregion
    }
}
