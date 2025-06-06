using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace PowerLab.Services
{
    public class SerilogService : Core.Contracts.ILogger
    {
        #region private members
        private static string LogFilePath(string LogEvent) => $@"{Environment.CurrentDirectory}\Logs\{LogEvent}\log.log";
        private static readonly string SerilogOutputTemplate = "{NewLine}{NewLine}日期：{Timestamp:yyyy-MM-dd HH:mm:ss.fff}{NewLine}日志级别：{Level}{NewLine}信息：{Message}{NewLine}{Exception}" + new string('-', 50);
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

        public void Debug(string message) => Logger.Debug(message);
        public void Information(string message) => Logger.Information(message);
        public void Warning(string message) => Logger.Warning(message);
        public void Error(string message) => Logger.Error(message);
        public void Fatal(string message) => Logger.Fatal(message);
    }
}
