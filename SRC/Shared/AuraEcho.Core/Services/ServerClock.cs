using System.Diagnostics;
using AuraEcho.Core.Contracts;

namespace AuraEcho.Core.Services;

public class ServerClock : IClock
{
    private long _serverUnixMsAtSync;
    private long _stopwatchMsAtSync;

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private volatile bool _isSynchronized;

    public DateTimeOffset UtcNow
    {
        get
        {
            if (!_isSynchronized)
                return DateTimeOffset.UtcNow;

            var elapsedMs = _stopwatch.ElapsedMilliseconds - Volatile.Read(ref _stopwatchMsAtSync);
            var serverNowMs = Volatile.Read(ref _serverUnixMsAtSync) + elapsedMs;

            return DateTimeOffset.FromUnixTimeMilliseconds(serverNowMs);
        }
    }

    public void Synchronize(DateTimeOffset serverUtcTime)
    {
        Interlocked.Exchange(ref _serverUnixMsAtSync, serverUtcTime.ToUnixTimeMilliseconds());
        Interlocked.Exchange(ref _stopwatchMsAtSync, _stopwatch.ElapsedMilliseconds);

        _isSynchronized = true;
    }
}
