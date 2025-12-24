using System.Net.Http;
using System.Text;
using PowerLab.Core.Contracts;

namespace PowerLab.Core.Tools.HttpClientPipelines;

public sealed class LoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        LogRequest(request);
        await LogResponse(response, cancellationToken);

        return response;
    }

    private void LogRequest(HttpRequestMessage request)
    {
        _logger.Debug($"HTTP {request.Method} {request.RequestUri}");

        foreach (var header in request.Headers)
            _logger.Debug($"  {header.Key}: {string.Join(",", header.Value)}");
    }

    private async Task LogResponse(HttpResponseMessage response, CancellationToken ct)
    {
        _logger.Debug($"HTTP {(int)response.StatusCode} {response.RequestMessage!.RequestUri}");

        foreach (var header in response.Headers)
            _logger.Debug($"  {header.Key}: {string.Join(",", header.Value)}");

        if (response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync(ct);
            _logger.Debug($"Response Body: {content}");

            response.Content = new StringContent(
                content,
                Encoding.UTF8,
                response.Content.Headers.ContentType?.MediaType);
        }
    }
}

