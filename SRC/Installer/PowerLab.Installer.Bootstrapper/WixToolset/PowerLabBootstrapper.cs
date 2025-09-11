using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper.WixToolset
{
    public sealed class PowerLabBootstrapper : BootstrapperApplication
    {
        private PowerLabBootstrapper() { }
        public static PowerLabBootstrapper Instance { get; } = new PowerLabBootstrapper();

        private Dispatcher _dispatcher;
        private const string PowerLabPackageId = "PowerLabInstallerMSI";
        private bool _isAutoPlan;

        public bool Downgrade { get; private set; }
        public IEngine Engine { get; private set; }
        public IBootstrapperCommand Command { get; private set; }
        private readonly object _syncRoot = new();
        public DetectionState DetectState { get; private set; }
        public string? ExistingVersion { get; private set; }
        public UpgradeDetectionState UpgradeDetectState { get; private set; }

        private int _progressPhases;
        private int _cacheProgress;
        private int _executeProgress;

        private WixStringVariable _installDirVar;
        private WixStringVariable _uninstallerPath;
        private WixBooleanVariable _createShortcutVar;
        private WixBooleanVariable _launchOnStartupVar;
        private WixStringVariable _versionVar;
        private WixStringVariable _bundleOriginalSource;

        /// <inheritdoc/>
        public event EventHandler? OnActionRequested;

        /// <inheritdoc/>
        public event EventHandler? OnActionCompleted;

        /// <inheritdoc/>
        public event EventHandler<int>? OnProgress;

        public event EventHandler<PlanMsiFeatureEventArgs> OnPlanMsiFeature;

        public event EventHandler<string>? OnExecuteMsiMessage;

        /// <inheritdoc/>
        public event EventHandler? OnCanceled;

        public string InstallDirectory
        {
            get => _installDirVar.Get();
            set => _installDirVar.Set(value);
        }

        public string UninstallerPath
        {
            get => _uninstallerPath.Get();
            set => _uninstallerPath.Set(value);
        }

        public bool CreateDesktopShortcut
        {
            get => _createShortcutVar.Get();
            set => _createShortcutVar.Set(value);
        }

        public bool LaunchOnStartup
        {
            get => _launchOnStartupVar.Get();
            set => _launchOnStartupVar.Set(value);
        }

        public string Version => _versionVar.Get();
        public string BundleOriginalSource => _bundleOriginalSource.Get();
        public bool CancelRequested { get; private set; }
        protected override void OnCreate(CreateEventArgs args)
        {
            base.OnCreate(args);
            Engine = args.Engine;
            Command = args.Command;

            Engine.Log(LogLevel.Standard, $"Command: {Command.Action} | Display: {Command.Display}");

            _installDirVar = new(Engine, BundleVar.InstallDirectory);
            _createShortcutVar = new(Engine, BundleVar.CreateDesktopShortcut);
            _launchOnStartupVar = new(Engine, BundleVar.LaunchOnStartup);
            _uninstallerPath = new(Engine, BundleVar.UninstallerPath);
            _versionVar = new(Engine, BundleVar.Version);
            _bundleOriginalSource = new(Engine, BundleVar.WixBundleOriginalSource);
            InitVariables();
            WireEvents();
        }

        private void InitVariables()
        {
            _installDirVar.Set(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "PowerLab"));
            _createShortcutVar.Set(true);
            _launchOnStartupVar.Set(true);

            var bundleOriginFileName = System.IO.Path.GetFileName(BundleOriginalSource);
            var uninstallPath =
                System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "Package Cache",
                    Engine.GetVariableString("WixBundleProviderKey"),
                    bundleOriginFileName);
            _uninstallerPath.Set(uninstallPath);
        }

        protected override void Run()
        {
            if (Environment.GetCommandLineArgs().Contains("-debug", StringComparer.OrdinalIgnoreCase))
                Debugger.Launch();

            // 稍后要在这里添加安装流程控制。
            Engine.Log(LogLevel.Standard, "Running the PowerLab.InstallerUI.");
            try
            {
                LaunchApp();
                Engine.Log(LogLevel.Standard, "Exiting the PowerLab.InstallerUI.");
                Engine.Quit(0);
            }
            catch (Exception ex)
            {
                Engine.Log(LogLevel.Error, $"The PowerLab.InstallerUI is failed: {ex}");
                Engine.Quit(-1);
            }
            finally
            {
                Engine.Log(LogLevel.Standard, "The PowerLab.InstallerUI has exited.");
            }
        }

        private void LaunchApp()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            if (Command.Display is Display.Passive or Display.Full)
            {
                var app = new App();
                app.InitializeComponent();
                //Engine.Detect();
                app.Run();
                return;
            }

            _isAutoPlan = true;
            Engine.Detect();
            Dispatcher.Run();
        }

        public void Install()
        {
            if (DetectState is DetectionState.Present
                && UpgradeDetectState is UpgradeDetectionState.None)
            {
                Plan(LaunchAction.Repair);
            }
            else
            {
                Plan(LaunchAction.Install);
            }
        }

        /// <inheritdoc/>
        public void Uninstall()
        {
            Plan(LaunchAction.Uninstall);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            CancelRequested = true;

        }

        /// <summary>
        /// Subscribes to all necessary bootstrapper engine events with their corresponding handlers.
        /// </summary>
        private void WireEvents()
        {
            base.DetectBegin += DetectBegin;
            base.DetectComplete += DetectComplete;
            base.DetectRelatedBundle += DetectRelatedBundle;
            base.DetectPackageComplete += DetectPackageComplete;
            base.PlanBegin += PlanBegin;
            base.PlanComplete += PlanComplete;
            base.ApplyBegin += ApplyBegin;
            base.Progress += Progress;
            base.ApplyComplete += ApplyComplete;
            base.Error += Error;
            base.ExecuteProgress += ExecuteProgress;
            base.PlannedPackage += PlannedPackage;
            base.CacheAcquireProgress += CacheAcquireProgress;
            base.CacheContainerOrPayloadVerifyProgress += CacheContainerOrPayloadVerifyProgress;
            base.CachePayloadExtractProgress += CachePayloadExtractProgress;
            base.CacheVerifyProgress += CacheVerifyProgress;
            base.CacheComplete += CacheComplete;
            base.PlanMsiFeature += PlanMsiFeature;
            base.ExecuteMsiMessage += ExecuteMsiMessage;
        }

        private void ExecuteMsiMessage(object? sender, ExecuteMsiMessageEventArgs e)
        {
            OnExecuteMsiMessage?.Invoke(this, e.Message);
        }

        private void PlanMsiFeature(object? sender, PlanMsiFeatureEventArgs e)
        {
            OnPlanMsiFeature?.Invoke(this, e);
        }

        /// <summary>
        /// Unsubscribes from all previously wired bootstrapper engine events.
        /// </summary>
        private void UnwireEvents()
        {
            base.DetectBegin -= DetectBegin;
            base.DetectComplete -= DetectComplete;
            base.DetectRelatedBundle -= DetectRelatedBundle;
            base.DetectPackageComplete -= DetectPackageComplete;
            base.PlanBegin -= PlanBegin;
            base.PlanComplete -= PlanComplete;
            base.ApplyBegin -= ApplyBegin;
            base.Progress -= Progress;
            base.ApplyComplete -= ApplyComplete;
            base.Error -= Error;
            base.ExecuteProgress -= ExecuteProgress;
            base.PlannedPackage -= PlannedPackage;
            base.CacheAcquireProgress -= CacheAcquireProgress;
            base.CacheContainerOrPayloadVerifyProgress -= CacheContainerOrPayloadVerifyProgress;
            base.CachePayloadExtractProgress -= CachePayloadExtractProgress;
            base.CacheVerifyProgress -= CacheVerifyProgress;
            base.CacheComplete -= CacheComplete;
        }

        /// <summary>
        /// Parses the command line arguments passed to the bootstrapper and updates corresponding bundle variables.
        /// </summary>
        private void ParseCommandLine()
        {
        }

        /// <summary>
        /// Sets the planned action for the bootstrapper and invokes the planning phase on the engine.
        /// </summary>
        /// <param name="action">The <see cref="LaunchAction"/> to plan.</param>
        private void Plan(LaunchAction action)
        {
            Engine.Plan(action);
        }

        /// <summary>
        /// Calculates overall progress percentage from cache and execution progress and raises the <see cref="OnProgress"/> event.
        /// </summary>
        private void ReportProgress()
        {
            var pct = GetProgress();

            if (CancelRequested) return;

            OnProgress?.Invoke(this, pct);
        }

        /// <summary>
        /// Computes the combined progress across caching and execution phases.
        /// </summary>
        /// <returns>The overall progress percentage.</returns>
        private int GetProgress()
        {
            return (_cacheProgress + _executeProgress) / _progressPhases;
        }

        #region Events Subscriptions

        /// <summary>
        /// Handles the beginning of package detection by setting the detection state and resetting the planned action.
        /// </summary>
        /// <param name="sender">The source of the detection event.</param>
        /// <param name="e">Event arguments containing registration type.</param>
        private void DetectBegin(object? sender, DetectBeginEventArgs e)
        {
            DetectState = RegistrationType.Full == e.RegistrationType ? DetectionState.Present : DetectionState.Absent;
        }

        /// <summary>
        /// Called when detection completes; parses CLI, updates state, raises action request, and invokes automatic plans.
        /// </summary>
        /// <param name="sender">The source of the detection completion event.</param>
        /// <param name="e">Event arguments containing status and resume type.</param>
        private void DetectComplete(object? sender, DetectCompleteEventArgs e)
        {
            if (Command.Action is not LaunchAction.Uninstall)
            {
                Downgrade = UpgradeDetectState == UpgradeDetectionState.Newer;
            }

            OnActionRequested?.Invoke(this, EventArgs.Empty);

            if (!_isAutoPlan) return;

            if (Command.Action == LaunchAction.Install)
            {
                Install();
                return;
            }

            if (Command.Action == LaunchAction.Uninstall)
            {
                Uninstall();
                return;
            }

            Engine.Log(LogLevel.Error, $"意外的自动计划：{Command.Action}");
        }

        /// <summary>
        /// Processes related bundle detection, sets existing version, and computes upgrade state.
        /// </summary>
        /// <param name="sender">The source of the related bundle event.</param>
        /// <param name="e">Event arguments containing related bundle details.</param>
        private void DetectRelatedBundle(object? sender, DetectRelatedBundleEventArgs e)
        {
            ExistingVersion = e.Version;

            if (e.RelationType is RelationType.Upgrade)
            {
                if (Engine.CompareVersions(Version, e.Version) >= 0)
                {
                    if (UpgradeDetectState == UpgradeDetectionState.None)
                    {
                        UpgradeDetectState = UpgradeDetectionState.Older;
                    }
                }
                else
                {
                    UpgradeDetectState = UpgradeDetectionState.Newer;
                }
            }
        }

        /// <summary>
        /// Updates existing version if the core package is already present.
        /// </summary>
        /// <param name="sender">The source of the package complete event.</param>
        /// <param name="e">Event arguments containing package ID and state.</param>
        private void DetectPackageComplete(object? sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId.Equals(PowerLabPackageId))
            {
                if (e.State is PackageState.Present)
                {
                    ExistingVersion = Version;
                }
            }
        }

        /// <summary>
        /// Clears the package order when planning begins.
        /// </summary>
        /// <param name="sender">The source of the plan begin event.</param>
        /// <param name="e">Event arguments containing phase count.</param>
        private void PlanBegin(object? sender, PlanBeginEventArgs e)
        {
        }

        /// <summary>
        /// Called when planning completes; initiates the apply phase or marks failure.
        /// </summary>
        /// <param name="sender">The source of the plan complete event.</param>
        /// <param name="args">Event arguments containing plan status.</param>
        private void PlanComplete(object? sender, PlanCompleteEventArgs args)
        {
            _dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current?.MainWindow ?? new Window();
                var hwnd = new WindowInteropHelper(mainWindow).EnsureHandle();
                Engine.Apply(hwnd);
            });
        }

        /// <summary>
        /// Captures the total number of execution phases when apply begins.
        /// </summary>
        /// <param name="sender">The source of the apply begin event.</param>
        /// <param name="e">Event arguments containing phase count.</param>
        private void ApplyBegin(object? sender, ApplyBeginEventArgs e)
        {
            _progressPhases = e.PhaseCount;
        }

        /// <summary>
        /// Cancels apply progress update if the operation is canceled.
        /// </summary>
        /// <param name="sender">The source of the progress event.</param>
        /// <param name="e">Event arguments containing progress percentage.</param>
        private void Progress(object? sender, ProgressEventArgs e)
        {
            e.Cancel = CancelRequested;
        }

        /// <summary>
        /// Called when application completes; finalizes state, raises action completed, and optionally closes UI.
        /// </summary>
        /// <param name="sender">The source of the apply complete event.</param>
        /// <param name="e">Event arguments containing apply status.</param>
        private void ApplyComplete(object? sender, ApplyCompleteEventArgs e)
        {
            OnActionCompleted?.Invoke(this, EventArgs.Empty);

            if (_isAutoPlan)
            {
                _dispatcher.InvokeShutdown();
                return;
            }
        }

        /// <summary>
        /// Handles engine errors, determining retry or cancel behavior based on state.
        /// </summary>
        /// <param name="sender">The source of the error event.</param>
        /// <param name="e">Event arguments containing error details.</param>
        private void Error(object? sender, ErrorEventArgs e)
        {

        }

        /// <summary>
        /// Updates execution progress, sends embedded progress if needed, and raises overall progress.
        /// </summary>
        /// <param name="sender">The source of the execute progress event.</param>
        /// <param name="e">Event arguments containing progress percentages.</param>
        private void ExecuteProgress(object? sender, ExecuteProgressEventArgs e)
        {
            lock (_syncRoot)
            {
                _executeProgress = e.OverallPercentage;
                ReportProgress();

                var progress = GetProgress();

                if (Command.Display is Display.Embedded)
                    Engine.SendEmbeddedProgress(e.ProgressPercentage, progress);

                e.Cancel = CancelRequested;
            }
        }

        /// <summary>
        /// Records each planned package with an execution order index.
        /// </summary>
        /// <param name="sender">The source of the planned package event.</param>
        /// <param name="e">Event arguments containing package ID and execute state.</param>
        private void PlannedPackage(object? sender, PlannedPackageEventArgs e)
        {

        }

        /// <summary>
        /// Updates cache acquisition progress and raises overall progress.
        /// </summary>
        /// <param name="sender">The source of the cache acquire event.</param>
        /// <param name="e">Event arguments containing overall percentage.</param>
        private void CacheAcquireProgress(object? sender, CacheAcquireProgressEventArgs e)
        {
            lock (_syncRoot)
            {
                _cacheProgress = e.OverallPercentage;
                ReportProgress();
                e.Cancel = CancelRequested;
            }
        }

        /// <summary>
        /// Updates cache verification progress and raises overall progress.
        /// <param name="sender">The source of the verify event.</param>
        /// <param name="e">Event arguments containing overall percentage.</param>
        private void CacheContainerOrPayloadVerifyProgress(object? sender, CacheContainerOrPayloadVerifyProgressEventArgs e)
        {
            lock (_syncRoot)
            {
                _cacheProgress = e.OverallPercentage;
                ReportProgress();
                e.Cancel = CancelRequested;
            }
        }

        /// <summary>
        /// Updates payload extract progress and raises overall progress.
        /// </summary>
        /// <param name="sender">The source of the payload extract event.</param>
        /// <param name="e">Event arguments containing overall percentage.</param>
        private void CachePayloadExtractProgress(object? sender, CachePayloadExtractProgressEventArgs e)
        {
            lock (_syncRoot)
            {
                _cacheProgress = e.OverallPercentage;
                ReportProgress();
                e.Cancel = CancelRequested;
            }
        }

        /// <summary>
        /// Updates cache verification progress and raises overall progress.
        /// </summary>
        /// <param name="sender">The source of the cache verify event.</param>
        /// <param name="e">Event arguments containing overall percentage.</param>
        private void CacheVerifyProgress(object? sender, CacheVerifyProgressEventArgs e)
        {
            lock (_syncRoot)
            {
                _cacheProgress = e.OverallPercentage;
                ReportProgress();
                e.Cancel = CancelRequested;
            }
        }

        /// <summary>
        /// Sets cache progress to 100% when caching completes and raises overall progress.
        /// </summary>
        /// <param name="sender">The source of the cache complete event.</param>
        /// <param name="e">Event arguments for cache completion.</param>
        private void CacheComplete(object? sender, CacheCompleteEventArgs e)
        {
            lock (_syncRoot)
            {
                _cacheProgress = 100;
                ReportProgress();
            }
        }

        #endregion
    }
}
