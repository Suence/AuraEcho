using Microsoft.Win32.TaskScheduler;

namespace PowerLab.Tools;

public class AutoStartManager
{
    public const string TaskName = "PowerLabAutoStart";

    /// <summary>
    /// 设置自启动状态
    /// </summary>
    /// <param name="enable">是否启用</param>
    public static void SetAutoStart(bool enable)
    {
        using var ts = new TaskService();
        if (enable)
        {
            if (ts.GetTask(TaskName) != null) return;

            string exePath = System.Environment.ProcessPath;

            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "PowerLab 的开机自动启动功能依赖此任务。如果此任务被禁用或删除，PowerLab 将无法在开机后自动启动。";

            td.Principal.RunLevel = TaskRunLevel.Highest;

            td.Triggers.Add(new LogonTrigger());

            td.Actions.Add(new ExecAction(exePath, "-hide", System.IO.Path.GetDirectoryName(exePath)));

            // 取消“只有在使用交流电源时才启动”
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;

            // 允许按需启动
            td.Settings.AllowDemandStart = true;

            ts.RootFolder.RegisterTaskDefinition(TaskName, td);
            return;
        }

        // 删除任务
        if (ts.GetTask(TaskName) != null)
        {
            ts.RootFolder.DeleteTask(TaskName);
        }
    }

    /// <summary>
    /// 检查当前是否已设置自启动
    /// </summary>
    public static bool IsAutoStartEnabled()
    {
        using var ts = new TaskService();
        return ts.GetTask(TaskName) != null;
    }
}
