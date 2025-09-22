namespace PowerLab.Core.Contracts;

/// <summary>
/// 文件对话框服务接口
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// 打开文件对话框
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    string? OpenFile(string dialogTitle, string filter = "All Files|*.*");

    /// <summary>
    /// 选择文件(多选)
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    string[] OpenFiles(string dialogTitle, string filter = "All Files|*.*");

    /// <summary>
    /// 打开文件夹对话框
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <returns></returns>
    string? SelectFolder(string dialogTitle);

    /// <summary>
    /// 选择目录(多选)
    /// </summary>
    /// <param name="dialogTitle"></param>
    /// <returns></returns>
    string[] SelectFolders(string dialogTitle);
}
