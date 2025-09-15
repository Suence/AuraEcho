using System;
using System.Collections.Generic;
using System.Linq;
using PowerLab.Core.Contracts;
using Rougamo;
using Rougamo.Context;

namespace PowerLab.Core.Attributes
{
    /// <summary>
    /// 标记一个方法，使其在执行前后或抛出异常时，打印日志。
    /// </summary>
    public class LoggingAttribute : MoAttribute
    {
        /// <summary>
        /// 日志记录器
        /// </summary>
        public static ILogger Logger { get; set; }

        /// <summary>
        /// 方法元数据缓存
        /// </summary>
        private readonly Dictionary<object, object> _metaDataCache = new Dictionary<object, object>();

        /// <summary>
        /// 方法执行前
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="Exception"></exception>
        public override void OnEntry(MethodContext context)
        {
            if (Logger == null)
                throw new Exception($"日志记录器：{nameof(LoggingAttribute)}.{nameof(Logger)} 为空");

            string parameters =
                String.Join(", ", context.Method.GetParameters().Select(p => $"{p.ParameterType} {p.Name}"));

            Logger.Debug($"Entry: {context.Method.DeclaringType.Name}.{context.Method.Name}({parameters})");
        }

        /// <summary>
        /// 方法执行异常
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="Exception"></exception>
        public override void OnException(MethodContext context)
        {
            if (Logger == null)
                throw new Exception($"日志记录器：{nameof(LoggingAttribute)}.{nameof(Logger)} 为空");

            Logger.Error($"Exception：{context.Exception}");
        }

        /// <summary>
        /// 方法退出时
        /// </summary>
        /// <param name="context"></param>
        public override void OnExit(MethodContext context)
        {
            base.OnExit(context);
        }

        /// <summary>
        /// 方法执行成功后
        /// </summary>
        /// <param name="context"></param>
        public override void OnSuccess(MethodContext context)
        {
            base.OnSuccess(context);
        }
    }
}
