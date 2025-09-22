using System.Management;
using System.Threading.Tasks;

namespace PowerLab.FishyTime.Utils;

public static class WMIHelper
{
    /// <summary>
    /// 查询显示器数量
    /// </summary>
    /// <returns></returns>
    public static int GetScreenCount()
    {
        string queryString = "SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'Monitor'";
        var screenCount = new ManagementObjectSearcher(queryString).Get().Count;
        return screenCount;
    }
    /// <summary>
    /// 查询显示器数量(此方法不会阻塞调用线程)
    /// </summary>
    /// <returns></returns>
    public static Task<int> GetScreenCountAsync()
        => Task.Run(GetScreenCount);
}
