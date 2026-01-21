using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using WixToolset.BootstrapperApplicationApi;
using ErrorEventArgs = WixToolset.BootstrapperApplicationApi.ErrorEventArgs;

namespace PowerLab.Installer.Bootstrapper.WixToolset;

public sealed partial class PowerLabBootstrapper : BootstrapperApplication
{
    private PowerLabBootstrapper() { }
    public static PowerLabBootstrapper Instance { get; } = new PowerLabBootstrapper();

    private Dispatcher _dispatcher;
    private const string PowerLabPackageId = "PowerLabInstallerMSI";
    private const string POWERLAB_BUNDLE_FILENAME = "PowerLabSetup.exe";
    private bool _isAutoPlan;
    private ExecuteMsiMessageEventArgs _currentAction;
    private ManualResetEventSlim _elevateLock = new(false);
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
    private WixStringVariable _bundleElevated;

    public event EventHandler? OnActionRequested;
    public event EventHandler? OnActionCompleted;
    public event EventHandler<int>? ProgressChanged;
    public event EventHandler<PlanMsiFeatureEventArgs> PlanFeature;
    public event EventHandler<string>? ExecuteMessage;
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
    public bool IsBundleElevated => _bundleElevated.Get() is "1";
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
        _bundleElevated = new(Engine, BundleVar.BundleElevated);
        InitVariables();
    }

    private void InitVariables()
    {
        string installDirRowValue = engine.GetVariableString(_installDirVar.WixVariableName);
        _installDirVar.Set(engine.FormatString(installDirRowValue));

        _createShortcutVar.Set(true);
        _launchOnStartupVar.Set(true);

        var uninstallPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Package Cache",
                Engine.GetVariableString("WixBundleProviderKey"),
                POWERLAB_BUNDLE_FILENAME);
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
            if (!IsBundleElevated)
            {
                Engine.Elevate(IntPtr.Zero);
            }
            else
            {
                _elevateLock.Set();
            }

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
        _elevateLock.Wait();

        if (!IsBundleElevated)
        {
            Engine.Log(LogLevel.Standard, "Elevated failed.");
            return;
        }

        _dispatcher = Dispatcher.CurrentDispatcher;
        if (Command.Display is Display.Passive or Display.Full)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
            return;
        }

        _isAutoPlan = true;
        Engine.Detect();
        Dispatcher.Run();
    }

    public void Install()
    {
        if (DetectState is DetectionState.Present &&
            UpgradeDetectState is UpgradeDetectionState.None)
        {
            Plan(LaunchAction.Repair);
        }
        else
        {
            Plan(LaunchAction.Install);
        }
    }

    public void Uninstall() => Plan(LaunchAction.Uninstall);

    public void Cancel() => CancelRequested = true;

    /// <inheritdoc/>
    protected override void OnApplyBegin(ApplyBeginEventArgs args)
    {
        base.OnApplyBegin(args);
        _progressPhases = args.PhaseCount;
    }

    /// <inheritdoc/>
    protected override void OnApplyComplete(ApplyCompleteEventArgs args)
    {
        base.OnApplyComplete(args);
        OnActionCompleted?.Invoke(this, EventArgs.Empty);

        if (_isAutoPlan)
        {
            _dispatcher.InvokeShutdown();
            return;
        }
    }

    /// <inheritdoc/>
    protected override void OnCacheAcquireProgress(CacheAcquireProgressEventArgs args)
    {
        base.OnCacheAcquireProgress(args);
        ExecuteMessage?.Invoke(this, "正在准备安装文件...");
        lock (_syncRoot)
        {
            _cacheProgress = args.OverallPercentage;
            ReportProgress();
            args.Cancel = CancelRequested;
        }
    }

    /// <inheritdoc/>
    protected override void OnCacheComplete(CacheCompleteEventArgs args)
    {
        base.OnCacheComplete(args);
        lock (_syncRoot)
        {
            _cacheProgress = 100;
            ReportProgress();
        }
    }

    /// <inheritdoc/>
    protected override void OnCacheContainerOrPayloadVerifyProgress(CacheContainerOrPayloadVerifyProgressEventArgs args)
    {
        base.OnCacheContainerOrPayloadVerifyProgress(args);

        ExecuteMessage?.Invoke(this, "正在验证组件完整性...");
        lock (_syncRoot)
        {
            _cacheProgress = args.OverallPercentage;
            ReportProgress();
            args.Cancel = CancelRequested;
        }
    }

    /// <inheritdoc/>
    protected override void OnCachePayloadExtractProgress(CachePayloadExtractProgressEventArgs args)
    {
        base.OnCachePayloadExtractProgress(args);
        lock (_syncRoot)
        {
            _cacheProgress = args.OverallPercentage;
            ReportProgress();
            args.Cancel = CancelRequested;
        }
    }

    /// <inheritdoc/>
    protected override void OnCacheVerifyProgress(CacheVerifyProgressEventArgs args)
    {
        base.OnCacheVerifyProgress(args);
        ExecuteMessage?.Invoke(this, "正在验证组件完整性...");
        lock (_syncRoot)
        {
            _cacheProgress = args.OverallPercentage;
            ReportProgress();
            args.Cancel = CancelRequested;
        }
    }

    /// <inheritdoc/>
    protected override void OnDetectBegin(DetectBeginEventArgs args)
    {
        base.OnDetectBegin(args);

        DetectState = RegistrationType.Full == args.RegistrationType ? DetectionState.Present : DetectionState.Absent;
    }

    /// <inheritdoc/>
    protected override void OnDetectComplete(DetectCompleteEventArgs args)
    {
        base.OnDetectComplete(args);
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

    /// <inheritdoc/>
    protected override void OnDetectPackageComplete(DetectPackageCompleteEventArgs args)
    {
        base.OnDetectPackageComplete(args);
        if (args.PackageId.Equals(PowerLabPackageId))
        {
            if (args.State is PackageState.Present)
            {
                ExistingVersion = Version;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnDetectRelatedBundle(DetectRelatedBundleEventArgs args)
    {
        base.OnDetectRelatedBundle(args);

        ExistingVersion = args.Version;

        if (args.RelationType is RelationType.Upgrade)
        {
            if (Engine.CompareVersions(Version, args.Version) >= 0)
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

    /// <inheritdoc/>
    protected override void OnDetectUpdate(DetectUpdateEventArgs args)
    {
        base.OnDetectUpdate(args);

        // TODO: 
    }

    /// <inheritdoc/>
    protected override void OnDetectUpdateBegin(DetectUpdateBeginEventArgs args)
    {
        base.OnDetectUpdateBegin(args);

        // TODO: 
    }

    /// <inheritdoc/>
    protected override void OnDetectUpdateComplete(DetectUpdateCompleteEventArgs args)
    {
        base.OnDetectUpdateComplete(args);

        // TODO: 
    }

    /// <inheritdoc/>
    protected override void OnElevateComplete(ElevateCompleteEventArgs args)
    {
        base.OnElevateComplete(args);

        _elevateLock.Set();
    }

    /// <inheritdoc/>
    protected override void OnError(ErrorEventArgs args)
    {
        base.OnError(args);
        ExecuteMessage?.Invoke(this, $"Error: {args.ErrorMessage}");
    }

    /// <inheritdoc/>
    protected override void OnExecuteFilesInUse(ExecuteFilesInUseEventArgs args)
    {
        base.OnExecuteFilesInUse(args);
    }

    /// <inheritdoc/>
    protected override void OnExecuteMsiMessage(ExecuteMsiMessageEventArgs args)
    {
        base.OnExecuteMsiMessage(args);

        args.Result = CancelRequested ? Result.Cancel : Result.Ok;

        string formattedMessage = FormatMessage(args);
        if (String.IsNullOrWhiteSpace(formattedMessage)) return;

        ExecuteMessage?.Invoke(this, formattedMessage);
    }

    private string FormatMessage(ExecuteMsiMessageEventArgs args)
    {
        if (args.MessageType == InstallMessage.ActionStart && args.Data.Count > 1)
        {
            _currentAction = args;

            string actionMessage = args.Data[1].Split(" ").First().TrimEnd('：', ':');
            return String.IsNullOrWhiteSpace(actionMessage) ? actionMessage : $"{actionMessage}...";
        }

        if (args.MessageType == InstallMessage.ActionData)
        {
            if (!DataIndexRegex().IsMatch(_currentAction.Data[1])) return String.Empty;

            if (!args.Message.StartsWith("1:")) return String.Empty;

            string result = DataIndexRegex().Replace(_currentAction.Data[1], m =>
            {
                if (int.TryParse(m.Groups[1].Value, out int index))
                {
                    int arrayIndex = index - 1;

                    if (arrayIndex >= 0 && arrayIndex < args.Data.Count)
                    {
                        return args.Data[arrayIndex];
                    }
                }
                return m.Value;
            });
            return result;
        }
        return String.Empty;
    }

    [GeneratedRegex(@"\[(\d+)\]")]
    private static partial Regex DataIndexRegex();

    /// <inheritdoc/>
    protected override void OnExecuteProgress(ExecuteProgressEventArgs args)
    {
        base.OnExecuteProgress(args);

        lock (_syncRoot)
        {
            _executeProgress = args.OverallPercentage;
            ReportProgress();

            var progress = GetProgress();

            if (Command.Display is Display.Embedded)
                Engine.SendEmbeddedProgress(args.ProgressPercentage, progress);

            args.Cancel = CancelRequested;
        }
    }

    /// <inheritdoc/>
    protected override void OnPlanBegin(PlanBeginEventArgs args)
    {
        base.OnPlanBegin(args);

        ExecuteMessage?.Invoke(this, "正在初始化...");
    }

    /// <inheritdoc/>
    protected override void OnPlanComplete(PlanCompleteEventArgs args)
    {
        base.OnPlanComplete(args);

        _dispatcher.Invoke(() =>
        {
            var mainWindow = Application.Current?.MainWindow ?? new Window();
            var hwnd = new WindowInteropHelper(mainWindow).EnsureHandle();
            Engine.Apply(hwnd);
        });
    }

    /// <inheritdoc/>
    protected override void OnPlanMsiFeature(PlanMsiFeatureEventArgs args)
    {
        base.OnPlanMsiFeature(args);

        PlanFeature?.Invoke(this, args);
    }

    /// <inheritdoc/>
    protected override void OnProgress(ProgressEventArgs args)
    {
        base.OnProgress(args);

        args.Cancel = CancelRequested;
    }

    /// <inheritdoc/>
    private void Plan(LaunchAction action)
    {
        Engine.Plan(action);
    }

    /// <inheritdoc/>
    private void ReportProgress()
    {
        var pct = GetProgress();

        if (CancelRequested) return;

        ProgressChanged?.Invoke(this, pct);
    }

    /// <inheritdoc/>
    private int GetProgress()
    {
        return (_cacheProgress + _executeProgress) / _progressPhases;
    }
}
