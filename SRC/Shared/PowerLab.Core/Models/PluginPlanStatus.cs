namespace PowerLab.Core.Models
{
    /// <summary>
    /// 模块状态
    /// </summary>
    public enum PluginPlanStatus
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        
        /// <summary>
        /// 启用挂起
        /// </summary>
        EnablePending,
        
        /// <summary>
        /// 禁用挂起
        /// </summary>
        DisablePending,

        /// <summary>
        /// 卸载挂起
        /// </summary>
        UninstallPending
    }
}
