namespace PowerLab.Core.Contracts
{
    public interface IFileDialogService
    {
        string? OpenFile(string dialogTitle, string filter = "All Files|*.*");
        string? SelectFolder(string dialogTitle);
    }
}
