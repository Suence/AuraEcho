using PowerLab.Core.Constants;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.IO;

namespace PowerLab.Core.Services;

/// <summary>
/// Serilog 日志服务
/// </summary>
public class SerilogService : Contracts.ILogger
{
    #region private members
    private static string LogFilePath(string LogEvent)
        => Path.Combine(ApplicationPaths.Logs, LogEvent, "log.log");

    /// <summary>
    /// 输出模板
    /// </summary>
    private static readonly string SerilogOutputTemplate = "{NewLine}{NewLine}日期：{Timestamp:yyyy-MM-dd HH:mm:ss.fff}{NewLine}日志级别：{Level}{NewLine}信息：{Message}{NewLine}{Exception}" + new string('-', 50);

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly Logger Logger =
        new LoggerConfiguration()
           .Enrich.FromLogContext()
           .MinimumLevel.Debug()
           .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.File(LogFilePath("Debug"), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
           .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.File(LogFilePath("Information"), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
           .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.File(LogFilePath("Warning"), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
           .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.File(LogFilePath("Error"), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
           .WriteTo.Logger(lg => lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.File(LogFilePath("Fatal"), rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate))
           .CreateLogger();
    #endregion

    /// <summary>
    /// Debug 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Debug(string message) => Logger.Debug(message);
    
    /// <summary>
    /// Information 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Information(string message) => Logger.Information(message);
    
    /// <summary>
    /// Warning 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Warning(string message) => Logger.Warning(message);
    
    /// <summary>
    /// Error 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Error(string message) => Logger.Error(message);

    /// <summary>
    /// Fatal 级别日志
    /// </summary>
    /// <param name="message"></param>
    public void Fatal(string message) => Logger.Fatal(message);
}
