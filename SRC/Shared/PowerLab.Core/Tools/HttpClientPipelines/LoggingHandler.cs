using System.Net.Http;
using PowerLab.Core.Contracts;

namespace PowerLab.Core.Tools.HttpClientPipelines;

public sealed class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.Debug($"HTTP {request.Method} {request.RequestUri}");

        if (request.Content != null)
        {
            var body = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.Debug($"Request Body: {body}");
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        _logger.Debug($"HTTP {(int)response.StatusCode} {request.RequestUri}");

        if (response.Content != null)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.Debug($"Response Body: {body}");
        }

        return response;
    }
}
