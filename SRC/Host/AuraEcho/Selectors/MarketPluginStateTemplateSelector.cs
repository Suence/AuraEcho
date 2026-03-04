using System;
using System.Windows;
using System.Windows.Controls;
using AuraEcho.PluginContracts.Models;

namespace AuraEcho.Selectors;

public class MarketPluginStateTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NotInstalledTemplate { get; set; }
    public DataTemplate? DownloadingTemplate { get; set; }
    public DataTemplate? InstallingTemplate { get; set; }
    public DataTemplate? InstalledTemplate { get; set; }
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is TransferStatus ts)
        {
            return ts switch
            {
                TransferStatus.None => NotInstalledTemplate,
                TransferStatus.Waiting => throw new NotImplementedException(),
                TransferStatus.Transferring => DownloadingTemplate,
                TransferStatus.Processing => InstallingTemplate,
                TransferStatus.Completed => InstalledTemplate,
                TransferStatus.Failed => throw new NotImplementedException(),
                _ => base.SelectTemplate(item, container),
            };
        }
        return base.SelectTemplate(item, container);
    }
}
