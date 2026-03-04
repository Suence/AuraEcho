using System;

namespace AuraEcho.FishyTime.Contracts;

public interface IHookManager : IDisposable
{
    public void StartHook();
    public void StopHook();
    public void ClearEventSubscribers();
}
