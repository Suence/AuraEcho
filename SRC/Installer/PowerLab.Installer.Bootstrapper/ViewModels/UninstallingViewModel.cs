using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using PowerLab.Installer.Bootstrapper.Constants;
using PowerLab.Installer.Bootstrapper.Extensions;
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

        /// <summary>
        /// 需要显示在WPFWindow
        /// </summary>
        private string message;
        private string _packageId = string.Empty;
        private bool canceled;
        private Dictionary<string, int> executingPackageOrderIndex;
        private string username;
        private int progress;
        private int cacheProgress;
        private int executeProgress;

        private int progressPhases = 1;
        private bool isUnstalling = false;
        #region Command

        public DelegateCommand UninstallCommand { get; }

        #endregion

        #region 属性
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public string CurrentDirectory
            => Process.GetCurrentProcess().MainModule.FileName;

        public string PackageId
        {
            get => _packageId;
            set => SetProperty(ref _packageId, "packid:" + value);
        }


        public int Progress
        {
            get => isUnstalling ? progress * 2 : progress;
            set
            {
                progress = value;
                RaisePropertyChanged(nameof(Progress));
                RaisePropertyChanged(nameof(Persent));
            }
        }

        public string Persent => Progress + "%";

        public int Phases => progressPhases;

        private string _installText = "Uninstall";
        public string InstallText
        {
            get => _installText;
            set => SetProperty(ref _installText, value);
        }

        public string RepairText { get; set; } = "Repair";

        private bool _lableback = true;
        public bool LabelBack
        {
            get => _lableback;
            set => SetProperty(ref _lableback, value);
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

            executingPackageOrderIndex = new Dictionary<string, int>();


            UninstallCommand = new DelegateCommand(() =>
            {
                _ba.Engine.Plan(LaunchAction.Uninstall);
                isUnstalling = true;
            });

           


            //进度条相关事件绑定
            //_ba.CacheAcquireProgress +=
            //(sender, args) =>
            //{
            //    this.cacheProgress = args.OverallPercentage;
            //    this.Progress = (this.cacheProgress + this.executeProgress) / 2;
            //};
            //_ba.ExecuteProgress +=
            //(sender, args) =>
            //{
            //    this.executeProgress = args.OverallPercentage;
            //    this.Progress = (this.cacheProgress + this.executeProgress) / 2;
            //};
            _ba.DetectPackageComplete += DetectPackageComplete;
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

            _ba.Engine.Detect();
        }

        #endregion

        private void DetectComplete(object sender, DetectCompleteEventArgs e)
        {
        }

        private void CacheAcquireProgress(object sender, CacheAcquireProgressEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(CacheAcquireProgress)}, OverallPercentage: {e.OverallPercentage}");
            //lock (this)
            //{
            //    cacheProgress = e.OverallPercentage;
            //    Progress = (cacheProgress + executeProgress) / Phases;
            //    // Wix4 已没有 Result
            //    e.Cancel = Canceled;
            //    //e.Result = Canceled ? Result.Cancel : Result.Ok;
            //}
        }
        private void ApplyExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ApplyExecuteProgress)}, OverallPercentage: {e.OverallPercentage}");
            lock (this)
            {

                executeProgress = e.OverallPercentage;
                Progress = (cacheProgress + executeProgress) / 2; // always two phases if we hit execution.

                if (_ba.Command.Display == Display.Embedded)
                {
                    _ba.Engine.SendEmbeddedProgress(e.ProgressPercentage, Progress);
                }

                //e.Cancel = Canceled;
            }
        }

        /// <summary>
        /// 这个方法 会在Detect中被调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
 
        }


        private void PlanBegin(object sender, PlanBeginEventArgs e)
        {
        }
        private void PlanPackageComplete(object sender, PlanPackageCompleteEventArgs e)
        {
        }

        /// <summary>
        /// PlanAction 结束后会触发这个方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PlanComplete(object sender, PlanCompleteEventArgs e)
        {
            _ba.Engine.Apply((IntPtr)Application.Current.Properties["MainWindowHandle"]);
        }
        /// <summary>
        /// ApplyAction 开始 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ApplyBegin(object sender, ApplyBeginEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ApplyBegin)}");
        }
        /// <summary>
        /// 安装
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ExecutePackageBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ExecutePackageBegin)}");

        }

        private void ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ExecuteMsiMessage)}");
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
            Debug.WriteLine($"Event: {nameof(CacheComplete)}");
            lock (this)
            {
                Progress = (cacheProgress + executeProgress) / progressPhases;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ExecutePackageComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ExecutePackageComplete)}");

        }
        /// <summary>
        /// Apply结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ApplyComplete)}");

            _regionManager.RequestNavigateOnUIThread(InstallerRegionNames.MainRegion, InstallerViewNames.UninstallFinish);
        }

        private void ApplyProgress(object sender, ProgressEventArgs e)
        {
            Debug.WriteLine($"Event: {nameof(ApplyProgress)}");
            //lock (this)
            //{
            //    e.Cancel = Canceled;
            //    //e.Result = Canceled ? Result.Cancel : Result.Ok;
            //}
        }
    }
}
