using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using PowerLab.Core.Constants;
using PowerLab.Host.Core.Models;
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

        public ObservableCollection<PluginRegistry> PluginRegistries
        {
            get => _pluginRegistries;
            set => SetProperty(ref _pluginRegistries, value);
        }

        public ObservableCollection<PluginRegistry> EnabledPlugins
            => new(PluginRegistries.Where(p => p.IsEnabled && !p.IsPending));

        public ObservableCollection<PluginRegistry> DisabledPlugins
            => new(PluginRegistries.Where(p => !p.IsEnabled && !p.IsPending));

        public ObservableCollection<PluginRegistry> PendingPlugins
            => new(PluginRegistries.Where(p => p.IsPending));

        public void PluginStatusChanged()
        {
            RaisePropertyChanged(nameof(EnabledPlugins));
            RaisePropertyChanged(nameof(DisabledPlugins));
            RaisePropertyChanged(nameof(PendingPlugins));
        }

        public DelegateCommand<PluginRegistry> EnablePluginCommand { get; }
        private void EnablePlugin(PluginRegistry pluginRegistry)
        {
            if (pluginRegistry == null) return;

            pluginRegistry.IsEnabled = true;
            pluginRegistry.IsPending = true;
            PluginStatusChanged();

            SaveData();
        }

        private void SaveData()
        {
            string pluginRegistriesJson = JsonSerializer.Serialize(PluginRegistries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(ApplicationPaths.Data, "plugin.registry.json"), pluginRegistriesJson);
        }

        public DelegateCommand<PluginRegistry> DisablePluginCommand { get; }
        private void DisablePlugin(PluginRegistry pluginRegistry)
        {
            if (pluginRegistry == null) return;

            pluginRegistry.IsEnabled = false;
            pluginRegistry.IsPending = true;
            PluginStatusChanged();

            SaveData();
        }

        public PluginsDashboardViewModel()
        {
            EnablePluginCommand = new DelegateCommand<PluginRegistry>(EnablePlugin);
            DisablePluginCommand = new DelegateCommand<PluginRegistry>(DisablePlugin);
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
            PluginStatusChanged();
        }
    }
}
