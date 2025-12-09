using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using PowerLab.Installer.Bootstrapper.Views;
using PowerLab.Installer.Bootstrapper.WixToolset;
using PowerLab.PluginContracts.Interfaces;
using PowerLab.UIToolkit.RegionDialog;
using Prism.Ioc;
using WixToolset.BootstrapperApplicationApi;

namespace PowerLab.Installer.Bootstrapper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly Mutex _mutex = new(false, "17FA29D6-F4BC-4720-A55C-27042D247E35");
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<PowerLabBootstrapper>(() => PowerLabBootstrapper.Instance);

        containerRegistry.Register<IRegionDialogService, RegionDialogService>();
        containerRegistry.RegisterForNavigation<ConfirmDialog>();

        containerRegistry.RegisterForNavigation<InstallPreparation>();
        containerRegistry.RegisterForNavigation<Installing>();
        containerRegistry.RegisterForNavigation<InstallFinish>();
        containerRegistry.RegisterForNavigation<UninstallPreparation>();
        containerRegistry.RegisterForNavigation<Uninstalling>();
        containerRegistry.RegisterForNavigation<UninstallFinish>();
        containerRegistry.RegisterForNavigation<ActionCancelled>();
        containerRegistry.RegisterForNavigation<DowngradeDetected>();
    }

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        try
        {
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                new InstallerAlreadyRunningDialog().ShowDialog();
                Shutdown();
                return;
            }
        }
        catch(AbandonedMutexException) { }

        RegisterEvents();

        base.OnStartup(e);
    }

    static void Main()
    {
        Debugger.Launch();
        ManagedBootstrapperApplication.Run(PowerLabBootstrapper.Instance);
    }

    /// <summary>
    /// 订阅全局异常处理事件
    /// </summary>
    private void RegisterEvents()
    {
        //Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        //UI线程未捕获异常处理事件（UI主线程）
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }
    /// <summary>
    /// Task 线程异常处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            if (e.Exception is Exception exception)
            {
                HandleException(exception);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {
            e.SetObserved();
        }
    }

    /// <summary>
    /// UI 线程异常处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            HandleException(e.Exception);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// 非 UI 线程异常处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception exception)
            {
                HandleException(exception);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        finally
        {

        }
    }

    /// <summary>
    /// 全局异常处理逻辑
    /// </summary>
    /// <param name="exception"></param>
    private void HandleException(Exception exception)
    {
        Debug.WriteLine(exception);
        PowerLabBootstrapper.Instance.Engine.Log(LogLevel.Error, exception.ToString());
    }
}
