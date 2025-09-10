using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.WixToolset;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper.ViewModels
{
    public class InstallPreparationViewModel : BindableBase
    {
        private readonly PowerLabBootstrapper _ba;
        private readonly IRegionManager _regionManager;
        private bool _agreeAgreement;
        private bool _isCreateDesktopFolderShortcut;
        private bool _isRunAtBoot;
        #region Command
        /// <summary>
        /// 打开协议声明
        /// </summary>
        public DelegateCommand OpenCloudServiceAgreementCommand { get; }
        private void OpenCloudServiceAgreement()
        {
            string currentFolderPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string cloudServiceAgreementFilePath = Path.Combine(currentFolderPath, "协议声明.pdf");
            Task.Run(() => Process.Start(cloudServiceAgreementFilePath));
        }

        /// <summary>
        /// 执行安装命令
        /// </summary>
        public DelegateCommand InstallCommand { get; }
        private bool CanInstall() => AgreeAgreement;
        private void Install()
        {
            _regionManager.RequestNavigate(
                InstallerRegionNames.MainRegion,
                InstallerViewNames.Installing,
                new NavigationParameters
                {
                    { "IsCreateDesktopFolderShortcut", IsCreateDesktopFolderShortcut },
                    { "IsRunAtBoot", IsRunAtBoot }
                });
        }

        #endregion

        #region 属性
        /// <summary>
        /// 是否创建桌面快捷方式
        /// </summary>
        public bool IsCreateDesktopFolderShortcut
        {
            get => _isCreateDesktopFolderShortcut;
            set => SetProperty(ref _isCreateDesktopFolderShortcut, value);
        }
        /// <summary>
        /// 开机自启
        /// </summary>
        public bool IsRunAtBoot
        {
            get => _isRunAtBoot;
            set => SetProperty(ref _isRunAtBoot, value);
        }
        /// <summary>
        /// 同意协议
        /// </summary>
        public bool AgreeAgreement
        {
            get => _agreeAgreement;
            set => SetProperty(ref _agreeAgreement, value);
        }

        public string Version => _ba.Version;

        public string TargetInstallFolder
        {
            get
            {
                return _ba.Engine.GetVariableString("InstallFolder");
            }

            set
            {
                var targetFolder = value;
                if (Directory.GetFiles(targetFolder).Length != 0 || Directory.GetDirectories(targetFolder).Length != 0)
                {
                    targetFolder = Path.Combine(targetFolder, "PowerLab");
                    return;
                }
                _ba.InstallDirectory = targetFolder;
                //_ba.Engine.SetVariableString(targetFolder, value, false);
            }
        }
        #endregion 

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="model"></param>
        public InstallPreparationViewModel(
            PowerLabBootstrapper ba,
            IRegionManager regionManager)
        {
            _ba = ba;
            _regionManager = regionManager; 

            IsCreateDesktopFolderShortcut = true;
            IsRunAtBoot = true;

            InstallCommand = new DelegateCommand(Install, CanInstall).ObservesProperty(() => AgreeAgreement);
            OpenCloudServiceAgreementCommand = new DelegateCommand(OpenCloudServiceAgreement);
        }
        #endregion
    }
}
