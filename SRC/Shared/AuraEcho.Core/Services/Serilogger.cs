using System.IO;
using AuraEcho.PluginContracts.Interfaces;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace AuraEcho.Core.Services;

/// <summary>
/// Serilog 日志服务
/// </summary>
public sealed class Serilogger : IAppLogger
{
    private readonly Logger _logger;

    public Serilogger(string logPath)
    {
        const string LOG_OUTPUT_TEMPLATE = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        const string DEBUG_PREFIX = "[AURAECHO]";
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .WriteTo.File(
                path: Path.Combine(logPath, "client-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,   // 保留 14 天
                fileSizeLimitBytes: 20 * 1024 * 1024, // 20MB 自动切分
                rollOnFileSizeLimit: true,
                shared: true,
                outputTemplate: LOG_OUTPUT_TEMPLATE)
            .WriteTo.Debug(outputTemplate: $"{DEBUG_PREFIX} {LOG_OUTPUT_TEMPLATE}")
            .CreateLogger();
    }

    /// <summary>
    /// Debug 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Debug(string message) => _logger.Debug(message);

    /// <summary>
    /// Information 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Information(string message) => _logger.Information(message);

    /// <summary>
    /// Warning 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message) => _logger.Warning(message);

    /// <summary>
    /// Error 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Error(string message) => _logger.Error(message);

    /// <summary>
    /// Fatal 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Fatal(string message) => _logger.Fatal(message);
}
