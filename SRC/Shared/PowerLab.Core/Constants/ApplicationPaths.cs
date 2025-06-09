using System.IO;

namespace PowerLab.Core.Constants
{
    public static class ApplicationPaths
    {
        public static string BasePath => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PowerLab");

        public static string Plugins => Path.Combine(BasePath, "plugins");
        public static string Logs => Path.Combine(BasePath, "logs");
        public static string Temp => Path.Combine(BasePath, "temp");

        static ApplicationPaths()
        {
            Directory.CreateDirectory(Plugins);
            Directory.CreateDirectory(Logs);
            Directory.CreateDirectory(Temp);
        }
    }
}
