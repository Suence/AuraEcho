using System.ComponentModel;
using PowerLab.PluginContracts.Models;

namespace PowerLab.PluginContracts.Interfaces;

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
