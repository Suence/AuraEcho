using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using PowerLab.Core.Contracts;
using PowerLab.Core.Events;
using Prism.Events;

namespace PowerLab.Core.Tools.HttpClientPipelines;

public class AuthHandler : DelegatingHandler
{
    private readonly IClientSession _clientSession;
    private readonly IEventAggregator _eventAggregator;

    public AuthHandler(IClientSession clientSession, IEventAggregator eventAggregator)
    {
        _clientSession = clientSession;
        _eventAggregator = eventAggregator;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _clientSession.AppToken?.AccessToken;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        if (_clientSession.AppToken is null) return response;

        if (!await _clientSession.TryRefreshTokenAsync())
        {
            _eventAggregator.GetEvent<SignInExpiredEvent>().Publish();
            return response;
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _clientSession.AppToken.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
