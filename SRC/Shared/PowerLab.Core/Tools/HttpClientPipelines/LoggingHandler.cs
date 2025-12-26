using System.Diagnostics;
using System.Net.Http;
using System.Text;
using PowerLab.PluginContracts.Interfaces;

namespace PowerLab.Core.Tools.HttpClientPipelines;

public sealed class LoggingHandler : DelegatingHandler
{
    private readonly IAppLogger _logger;

    public LoggingHandler(IAppLogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Request-Id", Guid.NewGuid().ToString("N"));

        var sw = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        sw.Stop();

        var logText = await BuildLogString(request, response, sw.Elapsed, cancellationToken);
        _logger.Debug(logText);

        return response;
    }

    private async Task<string> BuildLogString(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed, CancellationToken ct)
    {
        var sb = new StringBuilder(8192);

        sb.AppendLine();
        sb.AppendLine("┌─────────────────────────────────────────────────────────────────────────────");
        sb.AppendLine($"│ API Request: [{request.Method}] {request.RequestUri}");
        sb.AppendLine("├─────────────────────────────────────────────────────────────────────────────");
        
        // ========== Response ==========
        sb.AppendLine("├─ Request");
        foreach (var h in request.Headers)
            sb.AppendLine($"│  {h.Key}: {string.Join(", ", h.Value)}");

        if (request.Content != null)
        {
            foreach (var h in request.Content.Headers)
                sb.AppendLine($"│  {h.Key}: {string.Join(", ", h.Value)}");

            var body = await request.Content.ReadAsStringAsync(ct);
            if (!String.IsNullOrWhiteSpace(body))
            {
                sb.AppendLine("│  Body: " + body.Replace("\n", "\n│  "));
            }
        }

        sb.AppendLine("│");
        //sb.AppendLine("├─────────────────────────────────────────────────────────────────────────────");

        // ========== Response ==========
        sb.AppendLine("├─ Response");
        sb.AppendLine($"│  HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");

        foreach (var h in response.Headers)
            sb.AppendLine($"│  {h.Key}: {string.Join(", ", h.Value)}");

        if (response.Content != null)
        {
            foreach (var h in response.Content.Headers)
                sb.AppendLine($"│  {h.Key}: {string.Join(", ", h.Value)}");

            var body = await response.Content.ReadAsStringAsync(ct);
            if (!String.IsNullOrWhiteSpace(body))
            {
                sb.AppendLine("│  Body: " + body.Replace("\n", "\n│  "));
            }
        }

        sb.AppendLine($"└─────────────────────────────────Elapsed: {elapsed.TotalMilliseconds:0000} ms─────────────────────────────");

        return sb.ToString();
    }
}

