using Microsoft.WindowsAPICodePack.Dialogs;
using AuraEcho.Core.Contracts;

namespace AuraEcho.Core.Services;

/// <summary>
/// 文件对话框服务
/// </summary>
public class FileDialogService : IFileDialogService
{
    /// <summary>
    /// 打开文件对话框
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public string? OpenFile(string dialogTitle, string filter = "All Files|*.*")
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = dialogTitle,
            Filter = filter,
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <summary>
    /// 选择文件(多选)
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public string[] OpenFiles(string dialogTitle, string filter = "All Files|*.*")
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = dialogTitle,
            Filter = filter,
            Multiselect = true
        };
        return dialog.ShowDialog() == true ? dialog.FileNames : null;
    }

    /// <summary>
    /// 打开文件夹选择对话框
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <returns></returns>
    public string? SelectFolder(string dialogTitle)
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Title = dialogTitle,
            InitialDirectory = "C:\\"
        };

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            return dialog.FileName;
        }

        return null;
    }

    public string[] SelectFolders(string dialogTitle)
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true,
            Title = dialogTitle,
            Multiselect = true,
            InitialDirectory = "C:\\"
        };

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            return [.. dialog.FileNames];
        }

        return null;
    }
}
