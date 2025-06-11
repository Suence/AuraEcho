using Prism.Mvvm;

namespace PowerLab.Host.Core.Models
{
    public class PluginManifest : BindableBase
    {
        private string? _id;
        private string? _pluginName;
        private string? _version;
        private string? _description;
        private string? _entryAssemblyName;

        public string? Id 
        { 
            get => _id;
            set => SetProperty(ref _id, value); 
        }
        
        public string? PluginName 
        { 
            get => _pluginName;
            set => SetProperty(ref _pluginName, value);
        }

        public string? Version 
        { 
            get => _version; 
            set => SetProperty(ref _version, value);
        }

        public string? Description 
        { 
            get => _description; 
            set => SetProperty(ref _description, value);
        }

        public string? EntryAssemblyName 
        { 
            get => _entryAssemblyName; 
            set => SetProperty(ref _entryAssemblyName, value);
        }
    }
}
