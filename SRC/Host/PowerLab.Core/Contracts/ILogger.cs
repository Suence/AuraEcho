namespace PowerLab.Core.Contracts
{
    /// <summary>
    /// 日志服务
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Debug 级别日志
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);
        /// <summary>
        /// Information 级别日志
        /// </summary>
        /// <param name="message"></param>
        void Information(string message);
        /// <summary>
        /// Warning 级别日志
        /// </summary>
        /// <param name="message"></param>
        void Warning(string message);
        /// <summary>
        /// Error 级别日志
        /// </summary>
        /// <param name="message"></param>
        void Error(string message);
        /// <summary>
        /// Fatal 级别日志
        /// </summary>
        /// <param name="message"></param>
        void Fatal(string message);
    }
}
