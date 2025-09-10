using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
    public class UninstallingViewModel : BindableBase
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
        public DelegateCommand UninstallCommand { get; }
        private void Uninstall()
        {
            _ba.Uninstall();
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
        public UninstallingViewModel(PowerLabBootstrapper ba, IRegionManager regionManager)
        {
            _ba = ba;
            _regionManager = regionManager;

            UninstallCommand = new DelegateCommand(Uninstall);
            SubscriptionInstallEvents();
        }

        #endregion



        #region 方法

        private void SubscriptionInstallEvents()
        {
            _ba.OnActionCompleted += UninstallCompleted;
            _ba.OnExecuteMsiMessage += ExecuteMsiMessage;
            _ba.OnProgress += UpdateProgress;
        }

        private void ExecuteMsiMessage(object? sender, string e)
        {
            Message = e;
        }

        private void UpdateProgress(object? sender, int e)
        {
            Progress = e;
        }

        private void UninstallCompleted(object? sender, EventArgs e)
        {
            _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, InstallerViewNames.UninstallFinish);
        }

        private void UnsubscriptionInstallEvents()
        {
            _ba.OnActionCompleted -= UninstallCompleted;
            _ba.OnProgress -= UpdateProgress;
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
