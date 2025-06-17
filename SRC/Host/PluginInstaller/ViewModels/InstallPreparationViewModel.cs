using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using PluginInstaller.Constants;
using PluginInstaller.Tools;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Host.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PluginInstaller.ViewModels
{
    /// <summary>
    /// 安装准备
    /// </summary>
    public class InstallPreparationViewModel : BindableBase
    {
        #region private members
        private readonly ILogger? _logger;
        private readonly IRegionManager? _regionManager;

        private PluginManifest? _pluginManifest;
        private string _manifestFilePath;
        private string _tempExtractPath;
        private const string MANIFEST_FILE_NAME = "plugin.manifest.json";
        #endregion

        /// <summary>
        /// 模块清单信息
        /// </summary>
        public PluginManifest? PluginManifest
        {
            get => _pluginManifest;
            set => SetProperty(ref _pluginManifest, value);
        }

        public DelegateCommand LoadPluginCommand { get; }
        /// <summary>
        /// 加载模块清单
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        private void LoadPlugin()
        {
            string? filePath = GlobalObjectHolder.StartupArgs.FirstOrDefault();
            if (filePath is null || !File.Exists(filePath))
            {
                // TODO：交互
                _logger.Error("未指定插件文件或文件不存在。");
                return;
            }

            // 解压插件到临时目录
            _tempExtractPath = Path.Combine(ApplicationPaths.Temp, "PluginInstall_" + Guid.NewGuid());
            ZipFile.ExtractToDirectory(filePath, _tempExtractPath);

            // 读取并解析 manifest 文件
            string manifestPath = Path.Combine(_tempExtractPath, MANIFEST_FILE_NAME);
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("插件缺少 manifest 文件。");

            _manifestFilePath = manifestPath;
            string manifestJson = File.ReadAllText(_manifestFilePath);
            var manifest = JsonSerializer.Deserialize<PluginManifest>(manifestJson);

            if (manifest is null)
            {
                _logger.Error("插件 manifest 文件格式错误。");
                return;
            }

            PluginManifest = manifest;
        }

        public DelegateCommand BeginInstallCommand { get; }

        /// <summary>
        /// 开始安装
        /// </summary>
        private void BeginInstall()
        {
            if (PluginManifest is null)
            {
                _logger.Error("请先加载插件。");
                return;
            }
            _regionManager.RequestNavigate(
                RegionNames.MainRegion,
                ViewNames.Installing,
                new NavigationParameters
                {
                    { "PluginTempDir", _tempExtractPath },
                    { "PluginManifest", PluginManifest }
                });
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public InstallPreparationViewModel(IRegionManager regionManager, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));

            LoadPluginCommand = new DelegateCommand(LoadPlugin);
            BeginInstallCommand = new DelegateCommand(BeginInstall);
        }
    }
}
