using System.Text.Json.Serialization;
using Prism.Mvvm;

namespace PowerLab.Host.Core.Models
{
    public class PluginRegistry : BindableBase
    {
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private bool _isPending = false;
        [JsonIgnore]
        public bool IsPending
        {
            get => _isPending;
            set => SetProperty(ref _isPending, value);
        }

        public string DefaultView { get; set; }
        
        public string Name { get; set; }

        public string Id { get; set; }

        /// <summary>
        /// 插件所在目录路径
        /// </summary>
        public string PluginFolder { get; set; } = string.Empty;
    }
}
