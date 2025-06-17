using Prism.Mvvm;

namespace PowerLab.Host.Core.Models
{
    public class PluginRegistry : BindableBase
    {
        private PluginStatus _status;
        public PluginStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private PluginPlanStatus _planStatus;
        public PluginPlanStatus PlanStatus
        {
            get => _planStatus; 
            set => SetProperty(ref _planStatus, value);
        }

        private PluginManifest _manifest;
        public PluginManifest Manifest
        {
            get => _manifest;
            set => SetProperty(ref _manifest, value);
        }

        private string _defaultView;
        public string DefaultView
        {
            get => _defaultView;
            set => SetProperty(ref _defaultView, value);
        }

        /// <summary>
        /// 插件所在目录路径
        /// </summary>
        public string PluginFolder { get; set; } = string.Empty;
    }
}
