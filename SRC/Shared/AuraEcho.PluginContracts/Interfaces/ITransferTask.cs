using System.ComponentModel;
using AuraEcho.PluginContracts.Models;

namespace AuraEcho.PluginContracts.Interfaces;

public interface ITransferTask : INotifyPropertyChanged
{
    string Id { get; }
    string Name { get; }
    TransferType Type { get; }
    double Progress { get; }
    long TotalSize { get; }
    long TransferredSize { get; }
    TransferStatus Status { get; }

    Task Start();
    void Cancel();
}
