using System.Diagnostics;
using System.Windows;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.Extensions;
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
        private int _cacheProgress;
        private int _executeProgress;

        private bool _isCreateDesktopFolderShortcut;
        private bool _isRunAtBoot;
        #region Command
        /// <summary>
        /// 执行安装命令
        /// </summary>
        public DelegateCommand InstallCommand { get; }
        private void Install()
        {
            _ba.DetectPackageComplete += DetectPackageComplete;
            _ba.Engine.Detect();
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
        public int Progress => ExecuteProgress;

        /// <summary>
        /// 缓存进度
        /// </summary>
        public int CacheProgress
        {
            get => _cacheProgress;
            set
            {
                SetProperty(ref _cacheProgress, value);
                RaisePropertyChanged(nameof(Progress));
            }
        }
        /// <summary>
        /// 执行进度
        /// </summary>
        public int ExecuteProgress
        {
            get => _executeProgress;
            set
            {
                SetProperty(ref _executeProgress, value);
                RaisePropertyChanged(nameof(Progress));
            }
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
        }

        #endregion



        #region 方法

        private void SubscriptionInstallEvents()
        {
            _ba.PlanComplete += PlanComplete;
            _ba.ApplyComplete += ApplyComplete;
            _ba.ApplyBegin += ApplyBegin;
            _ba.ExecutePackageBegin += ExecutePackageBegin;
            _ba.ExecutePackageComplete += ExecutePackageComplete;
            // 当引擎获取安装源有进展时触发。
            _ba.CacheAcquireProgress += CacheAcquireProgress;
            // 在Payload上执行时由引擎触发。
            _ba.ExecuteProgress += ApplyExecuteProgress;
            // 当 Windows Installer 发送安装消息时触发。
            _ba.ExecuteMsiMessage += ExecuteMsiMessage;
            // 当引擎开始规划安装时触发。
            _ba.PlanBegin += PlanBegin;
            // 当引擎完成特定包的安装规划时触发。
            _ba.PlanPackageComplete += PlanPackageComplete;
            // 当引擎改变了捆绑包安装的进度时触发。
            _ba.Progress += ApplyProgress;
            // 引擎缓存安装源后触发。
            _ba.CacheComplete += CacheComplete;
            _ba.PlanMsiFeature += PlanMsiFeature;
        }

        /// <summary>
        /// 这个方法 会在Detect中被调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (!e.PackageId.Equals("PowerLabInstallerMSI", StringComparison.Ordinal)) return;
            if (e.State == PackageState.Present)
            {
                _ba.ApplyComplete += UninstallApplyComplete;
                _ba.PlanComplete += PlanComplete;
                _ba.Engine.Plan(LaunchAction.Uninstall);
            }
            if (e.State == PackageState.Absent)
            {
                SubscriptionInstallEvents();
                _ba.Engine.Plan(LaunchAction.Install);
                return;
            }
            //_regionManager.RequestNavigateOnUIThread(RegionNames.MainRegion, targetView);
        }

        private void UninstallApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            _ba.PlanComplete -= PlanComplete;
            _ba.ApplyComplete -= UninstallApplyComplete;
            _ba.Engine.Detect();
        }
        void PlanMsiFeature(object sender, PlanMsiFeatureEventArgs e)
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

        private void CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(CacheAcquireProgress)}, OverallPercentage: {e.OverallPercentage}");
            CacheProgress = e.OverallPercentage;
            //lock (this)
            //{
            //    cacheProgress = e.OverallPercentage;
            //    Progress = (cacheProgress + executeProgress) / Phases;
            //    e.Result = Canceled ? Result.Cancel : Result.Ok;
            //}
        }
        private void ApplyExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ApplyExecuteProgress)}, OverallPercentage: {e.OverallPercentage}");
            lock (this)
            {
                //executeProgress = e.OverallPercentage;
                //Progress = (cacheProgress + executeProgress) / 2; // always two phases if we hit execution.
                ExecuteProgress = e.OverallPercentage;

                if (_ba.Command.Display == Display.Embedded)
                {
                    _ba.Engine.SendEmbeddedProgress(e.ProgressPercentage, Progress);
                }

                //e.Result = Canceled ? Result.Cancel : Result.Ok;
            }
        }


        private void PlanBegin(object sender, PlanBeginEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(PlanBegin)}");
        }
        private void PlanPackageComplete(object sender, PlanPackageCompleteEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(PlanPackageComplete)}");
            // Wix4 已没有 ActionState 
            if (RequestState.Present != e.Requested) return;
            //if (ActionState.None == e.Execute) return;
        }

        /// <summary>
        /// PlanAction 结束后会触发这个方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(PlanComplete)}");
            //if (State == InstallState.Cancelled)
            //{
            //    CustomBootstrapperApplication.Dispatcher.InvokeShutdown();
            //    return;
            //}
            _ba.Engine.Apply((IntPtr)Application.Current.Properties["MainWindowHandle"]);
        }
        /// <summary>
        /// ApplyAction 开始 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ApplyBegin)}");
        }
        /// <summary>
        /// 安装
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ExecutePackageBegin)}");
            //if (State == InstallState.Cancelled)
            //{
            //    e.Result = Result.Cancel;
            //}
        }

        private void ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ExecuteMsiMessage)}");
            lock (this)
            {
                if (e.MessageType == InstallMessage.ActionStart)
                {
                    Message = e.Message;
                }

                //e.Result = Canceled ? Result.Cancel : Result.Ok;
            }
        }
        private void CacheComplete(object sender, CacheCompleteEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(CacheComplete)}");
            //lock (this)
            //{
            //    cacheProgress = 100;
            //    Progress = (cacheProgress + executeProgress) / progressPhases;
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ExecutePackageComplete)}");
            //if (State == InstallState.Cancelled)
            //{
            //    e.Result = Result.Cancel;
            //}
        }
        /// <summary>
        /// Apply结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ApplyComplete)}, Restart: {e.Restart}");
            await Task.Delay(TimeSpan.FromSeconds(2));
            _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, InstallerViewNames.InstallFinish);
        }

        private void ApplyProgress(object sender, ProgressEventArgs e)
        {
            Debug.WriteLine($"Method: {nameof(ApplyProgress)}, PP: {e.ProgressPercentage}, OP: {e.OverallPercentage}");
            //lock (this)
            //{
            //    e.Result = Canceled ? Result.Cancel : Result.Ok;
            //}
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
        }

        #endregion
    }
}
