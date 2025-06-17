using Microsoft.WindowsAPICodePack.Dialogs;
using PowerLab.Core.Contracts;

namespace PowerLab.Host.Core.Services
{
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
    }
}
