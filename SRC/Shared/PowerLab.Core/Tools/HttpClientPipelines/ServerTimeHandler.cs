using System.Diagnostics;
using System.Net.Http;
using PowerLab.Core.Contracts;

namespace PowerLab.Core.Tools.HttpClientPipelines;

public class ServerTimeHandler : DelegatingHandler
{
    private readonly IClock _clock;
    public ServerTimeHandler(IClock clock)
    {
        _clock = clock;
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        sw.Stop();

        if (response.Headers.Date.HasValue)
        {
            DateTimeOffset correctedServer = 
                response.Headers.Date.Value + 
                TimeSpan.FromMilliseconds(sw.Elapsed.TotalMilliseconds / 2);
            _clock.Synchronize(correctedServer);
        }

        return response;
    }
}
