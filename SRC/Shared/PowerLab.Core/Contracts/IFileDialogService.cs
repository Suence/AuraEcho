namespace PowerLab.Core.Contracts
{
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
        /// 打开文件夹对话框
        /// </summary>
        /// <param name="dialogTitle"></param>
        /// <returns></returns>
        string? SelectFolder(string dialogTitle);
    }
}
