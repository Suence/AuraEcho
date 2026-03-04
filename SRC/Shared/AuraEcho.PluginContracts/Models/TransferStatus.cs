namespace AuraEcho.PluginContracts.Models;

public enum TransferStatus
{
    None,
    Waiting,
    Transferring,
    Processing,
    Completed,
    Failed,
    Paused
}
