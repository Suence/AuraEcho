using Prism.Mvvm;

namespace PowerLab.Host.Core.Models
{
    /// <summary>
    /// 扩展模块清单
    /// </summary>
    public class PluginManifest : BindableBase
    {
        private string? _id;
        private string? _pluginName;
        private string? _version;
        private string? _description;
        private string? _entryAssemblyName;
        private string? _author;
        private string? _icon;

        /// <summary>
        /// Id
        /// </summary>
        public string? Id 
        { 
            get => _id;
            set => SetProperty(ref _id, value); 
        }
        
        /// <summary>
        /// 图标文件名称(icon.png)
        /// </summary>
        public string? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        /// <summary>
        /// 作者
        /// </summary>
        public string? Author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string? PluginName 
        { 
            get => _pluginName;
            set => SetProperty(ref _pluginName, value);
        }

        /// <summary>
        /// 版本
        /// </summary>
        public string? Version 
        { 
            get => _version; 
            set => SetProperty(ref _version, value);
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description 
        { 
            get => _description; 
            set => SetProperty(ref _description, value);
        }

        /// <summary>
        /// 入口程序集
        /// </summary>
        public string? EntryAssemblyName 
        { 
            get => _entryAssemblyName; 
            set => SetProperty(ref _entryAssemblyName, value);
        }
    }
}
