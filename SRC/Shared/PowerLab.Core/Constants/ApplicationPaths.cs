using System.IO;

namespace PowerLab.Core.Constants;

/// <summary>
/// 程序路径常量
/// </summary>
public static class ApplicationPaths
{
    public static string BasePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PowerLab");

    public static string Plugins => Path.Combine(BasePath, "plugins");
    public static string Logs => Path.Combine(BasePath, "logs");
    public static string Temp => Path.Combine(BasePath, "temp");
    public static string Data => Path.Combine(BasePath, "data");
    public static string HostSettings => Path.Combine(Data, "settings.json");
    public static string GetPluginPath(string pluginId) => Path.Combine(Plugins, pluginId);

    static ApplicationPaths()
    {
        Directory.CreateDirectory(Plugins);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(Data);
    }
}
