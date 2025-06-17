using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using PowerLab.Core.Constants;
using PowerLab.Host.Core.Models;
using PowerLab.Tools;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerLab.ViewModels
{
    public class PluginsDashboardViewModel : BindableBase, INavigationAware
    {
        #region private members
        private ObservableCollection<PluginRegistry> _pluginRegistries = [];
        #endregion

        /// <summary>
        /// 扩展模块列表
        /// </summary>
        public ObservableCollection<PluginRegistry> PluginRegistries
        {
            get => _pluginRegistries;
            set => SetProperty(ref _pluginRegistries, value);
        }

        /// <summary>
        /// 已启用的扩展模块列表
        /// </summary>
        public ObservableCollection<PluginRegistry> EnabledPlugins
            => new(PluginRegistries.Where(p => p.Status == PluginStatus.Enabled));

        /// <summary>
        /// 已禁用的扩展模块列表
        /// </summary>
        public ObservableCollection<PluginRegistry> DisabledPlugins
            => new(PluginRegistries.Where(p => p.Status == PluginStatus.Disabled));

        public DelegateCommand<PluginRegistry> PlanEnablePluginCommand { get; }
        private void PlanEnablePlugin(PluginRegistry pluginRegistry)
        {
            if (pluginRegistry is null) return;

            pluginRegistry.PlanStatus =
                pluginRegistry.Status == PluginStatus.Enabled
                ? PluginPlanStatus.None
                : PluginPlanStatus.EnablePending;

            SaveData();
        }

        /// <summary>
        /// 保存模块配置
        /// </summary>
        private void SaveData()
        {
            string pluginRegistriesJson = JsonSerializer.Serialize(PluginRegistries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(ApplicationPaths.Data, "plugin.registry.json"), pluginRegistriesJson);
        }

        public DelegateCommand<PluginRegistry> PlanDisablePluginCommand { get; }
        
        /// <summary>
        /// 计划禁用模块
        /// </summary>
        /// <param name="pluginRegistry"></param>
        private void PlanDisablePlugin(PluginRegistry pluginRegistry)
        {
            if (pluginRegistry == null) return;

            pluginRegistry.PlanStatus =
                pluginRegistry.Status == PluginStatus.Disabled
                ? PluginPlanStatus.None
                : PluginPlanStatus.DisablePending;

            SaveData();
        }


        public DelegateCommand<PluginRegistry> PlanUninstallPluginCommand { get; }
        
        /// <summary>
        /// 计划卸载模块
        /// </summary>
        /// <param name="pluginRegistry"></param>
        private void PlanUninstallPlugin(PluginRegistry pluginRegistry)
        {
            if (pluginRegistry == null) return;

            pluginRegistry.PlanStatus = PluginPlanStatus.UninstallPending;

            SaveData();
        }

        public DelegateCommand<PluginRegistry> CancelUninstallPluginCommand { get; }
        
        /// <summary>
        /// 取消卸载计划
        /// </summary>
        /// <param name="pluginRegistry"></param>
        private void CancelUninstallPlugin(PluginRegistry pluginRegistry)
        {
            if (pluginRegistry == null) return;

            pluginRegistry.PlanStatus = PluginPlanStatus.None;
            SaveData();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PluginsDashboardViewModel()
        {
            PlanEnablePluginCommand = new DelegateCommand<PluginRegistry>(PlanEnablePlugin);
            PlanDisablePluginCommand = new DelegateCommand<PluginRegistry>(PlanDisablePlugin);
            PlanUninstallPluginCommand = new DelegateCommand<PluginRegistry>(PlanUninstallPlugin);
            CancelUninstallPluginCommand = new DelegateCommand<PluginRegistry>(CancelUninstallPlugin);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
            => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            PluginRegistries =
                navigationContext.Parameters
                .GetValue<ObservableCollection<PluginRegistry>>("PluginRegistries")
                ?? [];

            RaisePropertyChanged(nameof(EnabledPlugins));
            RaisePropertyChanged(nameof(DisabledPlugins));
        }
    }
}
