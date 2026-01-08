using System;
using System.Security.Principal;
using Microsoft.Win32.TaskScheduler;
using WixToolset.Dtf.WindowsInstaller;

namespace PowerLabInstaller.CustomAction
{
    public class CustomActions
    {
        public const string TaskName = "PowerLabAutoStart";
        [CustomAction]
        public static ActionResult CreatePowerLabAutoStartTask(Session session)
        {
            session.Log("开始创建计划任务...");

            try
            {
                // 获取安装路径，注意：在 deferred 模式下需通过 CustomActionData 获取
                string installFolder = session.CustomActionData["INSTALLFOLDER"];
                string exePath = System.IO.Path.Combine(installFolder, "PowerLab.exe");

                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "PowerLab 的开机自动启动功能依赖此任务。如果此任务被禁用或删除，PowerLab 将无法在开机后自动启动。";

                    SecurityIdentifier adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                    td.Principal.GroupId = adminSid.Value;
                    td.Principal.RunLevel = TaskRunLevel.Highest;

                    td.Triggers.Add(new LogonTrigger());

                    td.Actions.Add(new ExecAction(exePath, "-hide", installFolder));

                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.ExecutionTimeLimit = TimeSpan.Zero;

                    ts.RootFolder.RegisterTaskDefinition(
                        TaskName,
                        td,
                        TaskCreation.CreateOrUpdate,
                        null,
                        null,
                        TaskLogonType.Group);
                }

                session.Log("计划任务创建成功。");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("创建计划任务失败: " + ex.ToString());
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult DeletePowerLabAutoStartTask(Session session)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    ts.RootFolder.DeleteTask(TaskName, false); 
                }
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("删除任务失败: " + ex.Message);
                return ActionResult.Success; // 卸载时通常建议忽略错误，防止卸载中断
            }
        }
    }
}