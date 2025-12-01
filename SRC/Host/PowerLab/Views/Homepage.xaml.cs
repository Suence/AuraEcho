using PowerLab.Core.Models;
using Prism;
using System;
using System.Windows.Controls;

namespace PowerLab.Views;

/// <summary>
/// Interaction logic for Homepage
/// </summary>
public partial class Homepage : UserControl
{
    public Homepage()
    {
        InitializeComponent();
    }

    private void EnabledPluginsFilter(object sender, System.Windows.Data.FilterEventArgs e)
    {
        if (e.Item is not PluginRegistry plugin)
        {
            e.Accepted = false;
            return;
        }

        e.Accepted = plugin.Status == PluginStatus.Enabled;
    }
}
