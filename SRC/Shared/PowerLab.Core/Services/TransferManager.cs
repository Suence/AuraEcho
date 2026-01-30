using System.Collections.ObjectModel;
using PowerLab.PluginContracts.Interfaces;
using Prism.Mvvm;

namespace PowerLab.Core.Services;

public class TransferManager : BindableBase, ITransferManager
{
    public ObservableCollection<ITransferTask> AllTasks { get; } = [];

    public async void AddTask(ITransferTask task)
    {
        if (AllTasks.Any(t => t.Id == task.Id)) return;

        AllTasks.Add(task);
        await task.Start();
    }

    public ITransferTask GetTaskById(string id)
        => AllTasks.FirstOrDefault(t => t.Id == id);
}
