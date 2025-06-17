using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Resources;
using PluginPacker.Models;
using PowerLab.Core.Contracts;
using PowerLab.Core.Extensions;
using PowerLab.Host.Core.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace PluginPacker.ViewModels
{
    public class HomepageViewModel : BindableBase
    {
        #region private members
        private readonly IFileDialogService _fileDialogService;
        private readonly ILogger _logger;

        private PluginFile _iconFile;
        private string _outputFolder;
        private ObservableCollection<PluginFile> _pluginFiles;
        private PluginManifest _pluginManifest;
        private string _manifestFileContent = string.Empty;
        #endregion

        public string ManifestFileContent
        {
            get => _manifestFileContent;
            set => SetProperty(ref _manifestFileContent, value);
        }

        public PluginManifest PluginManifest
        {
            get => _pluginManifest;
            set => SetProperty(ref _pluginManifest, value);
        }

        public string OutputFolder
        {
            get => _outputFolder;
            set => SetProperty(ref _outputFolder, value);
        }

        public ObservableCollection<PluginFile> PluginFiles
        {
            get => _pluginFiles;
            set => SetProperty(ref _pluginFiles, value);
        }

        public PluginFile IconFile
        {
            get => _iconFile;
            set => SetProperty(ref _iconFile, value);
        }

        public DelegateCommand<PluginFile> AddPluginFileCommand { get; }
        private void AddPluginFile(PluginFile pluginFile)
        {
            PluginFiles.Add(pluginFile);
        }

        public DelegateCommand<DragEventArgs> DropPluginFilesCommand { get; }
        private void DropPluginFiles(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files)
                return;

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                if (File.Exists(file) && !PluginFiles.Select(pf => pf.FileName).Contains(fileName))
                {
                    AddPluginFile(new PluginFile(file, fileName));
                }
            }
        }

        public DelegateCommand GenPluginIdCommand { get; }
        private void GenPluginId()
        {
            PluginManifest.Id = Guid.NewGuid().ToString("N").ToUpperInvariant();
            _logger.Debug($"生成插件 ID: {PluginManifest.Id}");
        }

        public DelegateCommand<PluginFile> SetEntryFileCommand { get; }
        private void SetEntryFile(PluginFile pluginFile)
        {
            PluginFiles.ForEach(pf => pf.IsEntryFile = false);
            pluginFile.IsEntryFile = true;

            PluginManifest.EntryAssemblyName = pluginFile.FileName;
        }

        public DelegateCommand OpenFileCommand { get; }
        private void OpenFile()
        {
            var filePath = _fileDialogService.OpenFile("选择插件文件", "所有文件 (*.*)|*.*");

            if (filePath is null) return;

            string? fileName = Path.GetFileName(filePath);
            if (!PluginFiles.Select(pf => pf.FileName).Contains(fileName))
            {
                AddPluginFile(new PluginFile(filePath, fileName));
            }
        }

        public DelegateCommand<PluginFile> RemovePluginFileCommand { get; }
        private void RemovePluginFile(PluginFile pluginFile)
        {
            PluginFiles.Remove(pluginFile);
        }

        public DelegateCommand SetIconCmmand { get; }
        private void SetIcon()
        {
            var filePath = _fileDialogService.OpenFile("选择图像文件", "图像文件|*.jpg;*.png;*.jpeg;*.bmp");

            if (filePath is null) return;

            string? fileName = Path.GetFileName(filePath);
            IconFile = new PluginFile(filePath, fileName);
            PluginManifest.Icon = IconFile.FileName;
        }

        public DelegateCommand SetOutputFolderCommand { get; }
        private void SetOutputFolder()
        {
            var folderPath = _fileDialogService.SelectFolder("选择输出目录");
            if (folderPath is not null)
            {
                OutputFolder = folderPath;
            }
        }

        public DelegateCommand PackPluginCommand { get; }
        private bool PackPluginCommandCanExecute()
        {
            return !String.IsNullOrWhiteSpace(PluginManifest.Id) &&
                   !String.IsNullOrWhiteSpace(PluginManifest.PluginName) &&
                   !String.IsNullOrWhiteSpace(PluginManifest.Version) &&
                   !String.IsNullOrWhiteSpace(PluginManifest.EntryAssemblyName) &&
                   !String.IsNullOrWhiteSpace(OutputFolder) &&
                   !String.IsNullOrWhiteSpace(PluginManifest.Author) &&
                   IconFile is not null;
        }
        private void PackPlugin()
        {
            _logger.Debug("开始打包");
            string pluginPackageFilePath = Path.Combine(OutputFolder, $"{PluginManifest.Id}.plix");

            byte[] manifestContentBytes = Encoding.UTF8.GetBytes(ManifestFileContent);

            // 创建或覆盖 zip 文件
            using (var zipStream = new FileStream(pluginPackageFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                var allPluginFiles = PluginFiles.Concat([IconFile]);
                foreach (var file in allPluginFiles)
                {
                    if (File.Exists(file.FilePath))
                    {
                        // 在压缩包中保留相对路径（可根据需要修改）
                        archive.CreateEntryFromFile(file.FilePath, file.FileName);
                    }
                    else
                    {
                        _logger.Debug($"文件未找到: {file.FilePath}");
                    }
                }

                // 创建一个新条目（即 zip 中的文件）
                var entry = archive.CreateEntry("plugin.manifest.json");

                // 向 entry 写入内存数据
                using var entryStream = entry.Open();
                entryStream.Write(manifestContentBytes, 0, manifestContentBytes.Length);
            }

            _logger.Debug("打包完成");
            MessageBox.Show("插件打包完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public HomepageViewModel(IFileDialogService fileDialogService, ILogger logger)
        {
            _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            PluginFiles = [];
            PluginManifest = new()
            {
                Author = "AUTHOR",
                PluginName = "PLUGIN NAME",
                Version = "1.0.0"
            };
            PluginManifest.PropertyChanged += PluginManifestChanged;

            AddPluginFileCommand = new DelegateCommand<PluginFile>(AddPluginFile);
            RemovePluginFileCommand = new DelegateCommand<PluginFile>(RemovePluginFile);
            PackPluginCommand = new DelegateCommand(PackPlugin, PackPluginCommandCanExecute);
            DropPluginFilesCommand = new DelegateCommand<DragEventArgs>(DropPluginFiles);
            OpenFileCommand = new DelegateCommand(OpenFile);
            SetEntryFileCommand = new DelegateCommand<PluginFile>(SetEntryFile);
            SetOutputFolderCommand = new DelegateCommand(SetOutputFolder);
            GenPluginIdCommand = new DelegateCommand(GenPluginId);
            SetIconCmmand = new DelegateCommand(SetIcon);

            PluginManifestChanged(null, null);
        }

        private void PluginManifestChanged(object? _, PropertyChangedEventArgs? __)
        {
            string pluginManifestContent =
                JsonSerializer.Serialize(PluginManifest, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            ManifestFileContent = pluginManifestContent;
            PackPluginCommand.RaiseCanExecuteChanged();
        }
    }
}
