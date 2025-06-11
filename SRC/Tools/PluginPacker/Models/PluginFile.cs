using Prism.Mvvm;

namespace PluginPacker.Models
{
    public class PluginFile : BindableBase
    {
        private bool _isEntryFile;

        public string FilePath { get; }
        public string FileName { get; }

        public bool IsEntryFile
        {
            get => _isEntryFile;
            set => SetProperty(ref _isEntryFile, value);
        }

        public PluginFile(string FilePath, string FileName, bool IsEntryFile = false)
        {
            this.FilePath = FilePath;
            this.FileName = FileName;
            this.IsEntryFile = IsEntryFile;
        }
    }
}
