using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using PowerLab.Core.Tools;
using PowerLab.PluginContracts.Attributes;
using PowerLab.PluginContracts.Interfaces;
using Prism.Ioc;

namespace PowerLab.Core.Services
{
    public class PluginInstallService : IPluginInstallService
    {
        private const string MANIFEST_FILE_NAME = "plugin.manifest.json";
        private readonly ILocalPluginRepository _localPluginRepository;
        private readonly IContainerProvider _containerProvider;
        private readonly ILogger _logger;
        public PluginInstallService(ILocalPluginRepository localPluginRepository, IContainerProvider containerProvider, ILogger logger)
        {
            _localPluginRepository = localPluginRepository;
            _containerProvider = containerProvider;
            _logger = logger;
        }

        private PluginRegistry InstallCore(string filePath)
        {
            // 解压插件到临时目录
            var extractPath = Path.Combine(ApplicationPaths.Temp, "PluginInstall_" + Guid.NewGuid());
            ZipFile.ExtractToDirectory(filePath, extractPath);

            // 读取并解析 manifest 文件
            string manifestPath = Path.Combine(extractPath, MANIFEST_FILE_NAME);
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("插件缺少 manifest 文件。");

            string manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestJson);

            // 拷贝到目标插件目录
            string finalPath = Path.Combine(ApplicationPaths.Plugins, manifest.Id);
            if (Directory.Exists(finalPath))
                Directory.Delete(finalPath, true);

            DirectoryUtils.SafeMoveDirectory(extractPath, finalPath);

            var existingPlugin = _localPluginRepository.GetPluginRegistries().FirstOrDefault(pr => pr.Manifest.Id == manifest.Id);
            if (existingPlugin is not null)
            {
                _localPluginRepository?.RemovePluginRegistry(existingPlugin.Id);
            }

            var entryAssemblyPath = Path.Combine(finalPath, manifest.EntryAssemblyName);
            var alc = new PluginLoadContext(entryAssemblyPath);
            Assembly pluginAssembly = null;
            try
            {
                pluginAssembly = alc.LoadFromAssemblyPath(entryAssemblyPath);
            }
            catch (Exception ex)
            {
                _logger.Error($"加载插件程序集失败：{manifest.PluginName}，异常：{ex.Message}");
                return null;
            }
            string defaultView = GetPluginDefaultView(pluginAssembly);

            IPluginSetup pluginDatabaseInitializer = GetPluginDatabaseInitializer(pluginAssembly);
            pluginDatabaseInitializer?.Setup(_containerProvider);
            var pluginRegistry = new PluginRegistry
            {
                Id = Guid.NewGuid().ToString(),
                PlanStatus = PluginPlanStatus.None,
                Manifest = manifest,
                PluginFolder = ApplicationPaths.GetPluginPath(manifest.Id),
                DefaultView = defaultView
            };
            _localPluginRepository.AddPluginRegistry(pluginRegistry);
            return pluginRegistry;

            IPluginSetup GetPluginDatabaseInitializer(Assembly pluginAssembly)
            {
                Type? pluginDatabaseInitializerType =
                    pluginAssembly.GetExportedTypes()
                                  .Where(t => typeof(IPluginSetup).IsAssignableFrom(t))
                                  .Where(t => t != typeof(IPluginSetup))
                                  .Where(t => !t.IsAbstract)
                                  .SingleOrDefault();
                return _containerProvider.Resolve(pluginDatabaseInitializerType) as IPluginSetup;
            }

            static string GetPluginDefaultView(Assembly pluginAssembly)
            {
                var targetAttribute = pluginAssembly.GetCustomAttributes<PluginDefaultViewAttribute>().FirstOrDefault() ?? throw new Exception("插件程序集没有指定 DefaultView");
                return targetAttribute.ViewName;
            }
        }

        public Task<PluginRegistry> InstallAsync(string filePath)
            => Task.Run(() => InstallCore(filePath));
    }
}
