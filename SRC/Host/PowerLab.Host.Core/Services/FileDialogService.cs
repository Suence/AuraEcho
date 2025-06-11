using Microsoft.WindowsAPICodePack.Dialogs;
using PowerLab.Core.Contracts;

namespace PowerLab.Host.Core.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string dialogTitle, string filter = "All Files|*.*")
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = dialogTitle,
                Filter = filter,
            };
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

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
