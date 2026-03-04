using AuraEcho.PluginContracts.Interfaces;
using Prism.Mvvm;

namespace AuraEcho.PluginContracts.Models;

public abstract class BaseTransferTask : BindableBase, ITransferTask
{
    public string Id { get; init; }
    public string Name { get; init; }
    public TransferType Type { get; init; }

    public double Progress
    {
        get => field;
        protected set => SetProperty(ref field, value);
    }

    public long TotalSize
    {
        get => field;
        protected set => SetProperty(ref field, value);
    }

    public long TransferredSize
    {
        get => field;
        protected set
        {
            if (SetProperty(ref field, value))
            {
                if (TotalSize > 0)
                    Progress = Math.Round((double)field / TotalSize * 100, 2);
            }
        }
    }

    public TransferStatus Status
    {
        get => field;
        protected set => SetProperty(ref field, value);
    }

    protected CancellationTokenSource _cts;

    protected BaseTransferTask(string id, string name, TransferType type)
    {
        Id = id;
        Name = name;
        Type = type;
    }

    public async Task Start()
    {
        if (Status == TransferStatus.Transferring) return;

        _cts = new CancellationTokenSource();
        try
        {
            Status = TransferStatus.Transferring;
            await ExecuteAsync(_cts.Token); 
            Status = TransferStatus.Completed;
        }
        catch (OperationCanceledException)
        {
            Status = TransferStatus.Paused; // or Canceled
        }
        catch (Exception)
        {
            Status = TransferStatus.Failed;
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }

    protected abstract Task ExecuteAsync(CancellationToken token);
}
