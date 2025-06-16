using System.Text.Json.Serialization;
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

        public string DefaultView { get; set; }
        
        public string Name { get; set; }

        public string Id { get; set; }

        /// <summary>
        /// 插件所在目录路径
        /// </summary>
        public string PluginFolder { get; set; } = string.Empty;
    }
}
