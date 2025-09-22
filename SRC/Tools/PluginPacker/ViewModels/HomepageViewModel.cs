using PluginPacker.Models;
using PowerLab.Core.Contracts;
using PowerLab.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
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
    private readonly ILogger _logger;

    private PluginFile _iconFile;
    private string _outputFolder;
    private PluginFile _entryFile;
    private PluginFolder _rootFolder;
    private PluginManifest _pluginManifest;
    private string _manifestFileContent = string.Empty;
    #endregion

    /// <summary>
    /// 模块清单内容
    /// </summary>
    public string ManifestFileContent
    {
        get => _manifestFileContent;
        set => SetProperty(ref _manifestFileContent, value);
    }

    /// <summary>
    /// 模块清单
    /// </summary>
    public PluginManifest PluginManifest
    {
        get => _pluginManifest;
        set => SetProperty(ref _pluginManifest, value);
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
        set
        {
            SetProperty(ref _entryFile, value);
            PluginManifest.EntryAssemblyName = value?.Name;
        }
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

    public DelegateCommand GenPluginIdCommand { get; }
    /// <summary>
    /// 生成模块 ID
    /// </summary>
    private void GenPluginId()
    {
        PluginManifest.Id = Guid.NewGuid().ToString("N").ToUpperInvariant();
        _logger.Debug($"生成插件 ID: {PluginManifest.Id}");
    }

    public DelegateCommand<PluginFile> SetEntryFileCommand { get; }
    /// <summary>
    /// 设置入口程序集
    /// </summary>
    /// <param name="pluginFile"></param>
    private void SetEntryFile(PluginFile pluginFile)
    {
        EntryFile = pluginFile;
    }

    public DelegateCommand SetIconCmmand { get; }
    /// <summary>
    /// 设置模块图标
    /// </summary>
    private void SetIcon()
    {
        var filePath = _fileDialogService.OpenFile("选择图像文件", "图像文件|*.jpg;*.png;*.jpeg;*.bmp");

        if (filePath is null) return;

        string? fileName = Path.GetFileName(filePath);
        IconFile = new PluginFile(filePath, fileName, RootFolder);
        PluginManifest.Icon = IconFile.Name;
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
        return !String.IsNullOrWhiteSpace(PluginManifest.Id) &&
               !String.IsNullOrWhiteSpace(PluginManifest.PluginName) &&
               !String.IsNullOrWhiteSpace(PluginManifest.Version) &&
               !String.IsNullOrWhiteSpace(PluginManifest.EntryAssemblyName) &&
               !String.IsNullOrWhiteSpace(OutputFolder) &&
               !String.IsNullOrWhiteSpace(PluginManifest.Author) &&
               IconFile is not null;
    }

    /// <summary>
    /// 开始打包
    /// </summary>
    private void PackPlugin()
    {
        _logger.Debug("开始打包");
        string pluginPackageFilePath = Path.Combine(OutputFolder, $"{PluginManifest.Id}.plix");

        byte[] manifestContentBytes = Encoding.UTF8.GetBytes(ManifestFileContent);

        using (var zipStream = new FileStream(pluginPackageFilePath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            // 递归写入 RootPluginFolder
            AddFolderToArchive(RootFolder, archive, "");

            // 写入 IconFile（如果有单独的图标）
            if (IconFile is not null && File.Exists(IconFile.FilePath))
            {
                archive.CreateEntryFromFile(IconFile.FilePath, IconFile.Name);
            }

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

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileDialogService"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public HomepageViewModel(IFileDialogService fileDialogService, ILogger logger)
    {
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _rootFolder = new(String.Empty, null) { Name = "插件文件" };

        PluginManifest = new()
        {
            Author = "AUTHOR",
            PluginName = "PLUGIN NAME",
            Version = "1.0.0"
        };
        PluginManifest.PropertyChanged += PluginManifestChanged;

        DropFilesCommand = new DelegateCommand<PluginFolder>(DropFiles);
        AddPluginFileCommand = new DelegateCommand<PluginFolder>(AddPluginFile);
        AddPluginFolderCommand = new DelegateCommand<PluginFolder>(AddPluginFolder);
        RemoveFileCommand = new DelegateCommand<PluginFile>(RemoveFile);
        RemoveFolderCommand = new DelegateCommand<PluginFolder>(RemoveFolder);

        PackPluginCommand = new DelegateCommand(PackPlugin, PackPluginCommandCanExecute);
        SetEntryFileCommand = new DelegateCommand<PluginFile>(SetEntryFile);
        SetOutputFolderCommand = new DelegateCommand(SetOutputFolder);
        GenPluginIdCommand = new DelegateCommand(GenPluginId);
        SetIconCmmand = new DelegateCommand(SetIcon);

        PluginManifestChanged(null, null);
    }

    /// <summary>
    /// 模块清单信息已更改
    /// </summary>
    /// <param name="_"></param>
    /// <param name="__"></param>
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
