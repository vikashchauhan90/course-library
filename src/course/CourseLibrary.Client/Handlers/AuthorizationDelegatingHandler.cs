using CourseLibrary.Client.Security;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace CourseLibrary.Client.Handlers;



internal sealed class AuthorizationDelegatingHandler(
    IAccessTokenProvider accessTokenProvider,
    ILogger<AuthorizationDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await accessTokenProvider
            .GetAccessTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            logger.LogWarning(
                "No access token available for outgoing HTTP request {Method} {Uri}.",
                request.Method,
                request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        logger.LogInformation (
            "Authorization header added to outgoing HTTP request {Method} {Uri}.",
            request.Method,
            request.RequestUri);

        return await base.SendAsync(request, cancellationToken);
    }
}
