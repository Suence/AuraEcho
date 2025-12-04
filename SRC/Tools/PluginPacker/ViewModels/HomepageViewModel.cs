using PluginPacker.Models;
using PowerLab.Core.Constants;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;

namespace PluginPacker.ViewModels;

/// <summary>
/// 首页
/// </summary>
public class HomepageViewModel : BindableBase
{
    #region private members
    private readonly IFileDialogService _fileDialogService;
    private readonly IFileRepository _fileRepository;
    private readonly IRemotePluginRepository _remotePluginRepository;
    private readonly ILogger _logger;

    private PluginFile _iconFile;
    private string _outputFolder;
    private PluginFile _entryFile;
    private PluginFolder _rootFolder;
    private string _manifestFileContent = string.Empty;
    #endregion

    public ObservableCollection<AppPlugin> Plugins
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public AppPlugin CurrentPlugin
    {
        get => field;
        set
        {
            SetProperty(ref field, value);
            if (value is null)
            {
                return;
            }
            _ = LoadPluginDetailsAsync(value);
        }
    }

    public string Version
    {
        get => field;
        set
        {
            SetProperty(ref field, value);
            BuildPluginManifestConetnt();
        }
    }

    /// <summary>
    /// 模块清单内容
    /// </summary>
    public string ManifestFileContent
    {
        get => _manifestFileContent;
        set => SetProperty(ref _manifestFileContent, value);
    }


    /// <summary>
    /// 模块包输出目录
    /// </summary>
    public string OutputFolder
    {
        get => _outputFolder;
        set => SetProperty(ref _outputFolder, value);
    }

    public PluginFile EntryFile
    {
        get => _entryFile;
        set => SetProperty(ref _entryFile, value);
    }

    public PluginFolder RootFolder
    {
        get => _rootFolder;
        set => SetProperty(ref _rootFolder, value);
    }

    /// <summary>
    /// 图标文件
    /// </summary>
    public PluginFile IconFile
    {
        get => _iconFile;
        set => SetProperty(ref _iconFile, value);
    }

    public DelegateCommand<PluginFolder> DropFilesCommand { get; }
    private void DropFiles(PluginFolder pluginFolder)
    {

    }

    public DelegateCommand<PluginFile> RemoveFileCommand { get; }
    private void RemoveFile(PluginFile pluginFile)
    {
        pluginFile.Parent.Children.Remove(pluginFile);

        if (pluginFile != EntryFile) return;

        EntryFile = null;
    }

    public DelegateCommand<PluginFolder> RemoveFolderCommand { get; }
    private void RemoveFolder(PluginFolder pluginFolder)
    {
        pluginFolder.Parent.Children.Remove(pluginFolder);
    }

    public DelegateCommand<PluginFolder> AddPluginFileCommand { get; }

    /// <summary>
    /// 添加模块文件
    /// </summary>
    /// <param name="pluginFile"></param>
    private void AddPluginFile(PluginFolder targetFolder)
    {
        var filePaths = _fileDialogService.OpenFiles("选择插件文件", "所有文件 (*.*)|*.*");

        if (filePaths is null || !filePaths.Any()) return;

        foreach (string filePath in filePaths)
        {
            string? fileName = Path.GetFileName(filePath);
            if (!targetFolder.Children.OfType<PluginFile>().Select(pf => pf.Name).Contains(fileName))
            {
                targetFolder.Add(new PluginFile(filePath, fileName, targetFolder));
            }
        }
    }

    public DelegateCommand<PluginFolder> AddPluginFolderCommand { get; }
    private void AddPluginFolder(PluginFolder targetFolder)
    {
        string[]? folderPaths = _fileDialogService.SelectFolders("选择目录");
        if (folderPaths is null || folderPaths.Length <= 0) return;

        foreach (string folderPath in folderPaths)
        {
            if (targetFolder.Children.OfType<PluginFolder>().Select(folder => folder.FolderPath).Contains(folderPath))
                continue;

            targetFolder.Add(new PluginFolder(folderPath, targetFolder));
        }
    }

    public DelegateCommand<PluginFile> SetEntryFileCommand { get; }
    /// <summary>
    /// 设置入口程序集
    /// </summary>
    /// <param name="pluginFile"></param>
    private void SetEntryFile(PluginFile pluginFile)
    {
        EntryFile = pluginFile;
        BuildPluginManifestConetnt();
    }

    public DelegateCommand SetOutputFolderCommand { get; }
    /// <summary>
    /// 设置输出目录
    /// </summary>
    private void SetOutputFolder()
    {
        var folderPath = _fileDialogService.SelectFolder("选择输出目录");
        if (folderPath is not null)
        {
            OutputFolder = folderPath;
        }
    }

    public DelegateCommand PackPluginCommand { get; }
    /// <summary>
    /// 可打包
    /// </summary>
    /// <returns></returns>
    private bool PackPluginCommandCanExecute()
    {
        return !String.IsNullOrWhiteSpace(Version) &&
                EntryFile is not null &&
               !String.IsNullOrWhiteSpace(OutputFolder);
    }

    /// <summary>
    /// 开始打包
    /// </summary>
    private async void PackPlugin()
    {
        _logger.Debug("开始打包");

        await _fileRepository.DownloadFileAsync(CurrentPlugin.IconFileId, IconFile.FilePath, null);

        string pluginPackageFilePath = Path.Combine(OutputFolder, $"{CurrentPlugin.Name}.plix");

        byte[] manifestContentBytes = Encoding.UTF8.GetBytes(ManifestFileContent);

        using (var zipStream = new FileStream(pluginPackageFilePath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            // 递归写入 RootPluginFolder
            AddFolderToArchive(RootFolder, archive, "");

            // 写入 IconFile（如果有单独的图标）
            archive.CreateEntryFromFile(IconFile.FilePath, IconFile.Name);

            // 写入 manifest
            var entry = archive.CreateEntry("plugin.manifest.json");
            using var entryStream = entry.Open();
            entryStream.Write(manifestContentBytes, 0, manifestContentBytes.Length);
        }

        _logger.Debug("打包完成");
        MessageBox.Show("插件打包完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 递归添加文件夹及其内容到压缩包
    /// </summary>
    private void AddFolderToArchive(PluginFolder folder, ZipArchive archive, string relativePath)
    {
        // 添加文件
        foreach (var file in folder.Children.OfType<PluginFile>())
        {
            if (File.Exists(file.FilePath))
            {
                string entryName = Path.Combine(relativePath, file.Name);
                archive.CreateEntryFromFile(file.FilePath, entryName);
            }
            else
            {
                _logger.Debug($"文件未找到: {file.FilePath}");
            }
        }

        // 递归子文件夹
        foreach (var subFolder in folder.Children.OfType<PluginFolder>())
        {
            AddFolderToArchive(subFolder, archive, Path.Combine(relativePath, subFolder.Name));
        }
    }

    private async Task LoadPluginsAsync()
    {
        var result = await _remotePluginRepository.GetPluginsAsync();
        if (result is null) return;

        Plugins = [.. result];
        CurrentPlugin = Plugins.FirstOrDefault();
    }

    private async Task LoadPluginDetailsAsync(AppPlugin plugin)
    {
        var fileInfo = await _fileRepository.GetFileByIdAsync(CurrentPlugin.IconFileId);
        IconFile = new PluginFile(Path.Combine(ApplicationPaths.Temp, fileInfo.FileName).Replace("\\", "/"), fileInfo.FileName, RootFolder);

        var latestVersionPackage = await _remotePluginRepository.GetLatestAsync(plugin.Id);
        var latestVersion = new Version(latestVersionPackage?.Version ?? "1.0.0");
        Version = $"{latestVersion.Major}.{latestVersion.Minor}.{latestVersion.Build + 1}";
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileDialogService"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public HomepageViewModel(IFileRepository fileRepository, IRemotePluginRepository remotePluginRepository, IFileDialogService fileDialogService, ILogger logger)
    {
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        _remotePluginRepository = remotePluginRepository ?? throw new ArgumentNullException(nameof(remotePluginRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _rootFolder = new(String.Empty, null) { Name = "插件文件" };

        DropFilesCommand = new DelegateCommand<PluginFolder>(DropFiles);
        AddPluginFileCommand = new DelegateCommand<PluginFolder>(AddPluginFile);
        AddPluginFolderCommand = new DelegateCommand<PluginFolder>(AddPluginFolder);
        RemoveFileCommand = new DelegateCommand<PluginFile>(RemoveFile);
        RemoveFolderCommand = new DelegateCommand<PluginFolder>(RemoveFolder);

        PackPluginCommand =
            new DelegateCommand(PackPlugin, PackPluginCommandCanExecute)
                .ObservesProperty(() => OutputFolder)
                .ObservesProperty(() => Version)
                .ObservesProperty(() => EntryFile);

        SetEntryFileCommand = new DelegateCommand<PluginFile>(SetEntryFile);
        SetOutputFolderCommand = new DelegateCommand(SetOutputFolder);

        _ = LoadPluginsAsync();
    }

    private void BuildPluginManifestConetnt()
    {
        string pluginManifestContent =
            JsonSerializer.Serialize(new PluginManifest
            {
                Author = CurrentPlugin.Author,
                Description = CurrentPlugin.Description,
                EntryAssemblyName = EntryFile?.Name,
                Icon = IconFile.Name,
                Id = CurrentPlugin.Id,
                PluginName = CurrentPlugin.DisplayName,
                Version = Version
            }, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        ManifestFileContent = pluginManifestContent;
        PackPluginCommand.RaiseCanExecuteChanged();
    }
}
