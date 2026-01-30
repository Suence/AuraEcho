using System.Collections.ObjectModel;

namespace PowerLab.PluginContracts.Interfaces;

public interface ITransferManager
{
    ObservableCollection<ITransferTask> AllTasks { get; }
    void AddTask(ITransferTask task);
    ITransferTask GetTaskById(string id);
}
