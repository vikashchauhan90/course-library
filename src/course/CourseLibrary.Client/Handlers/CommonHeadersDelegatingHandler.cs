using CourseLibrary.Client.Observability;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Client.Handlers;


internal sealed class CommonHeadersDelegatingHandler(
    ICommonHeadersProvider headersProvider,
    ILogger<CommonHeadersDelegatingHandler> logger) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var headers = headersProvider.GetHeaders();

        foreach (var header in headers ?? Enumerable.Empty<KeyValuePair<string, string>>())
        {
            request.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value);
        }

        logger.LogDebug(
            "Added {HeaderCount} common headers to outgoing HTTP request {Method} {Uri}.",
            headers?.Count() ?? 0,
            request.Method,
            request.RequestUri);

        return base.SendAsync(request, cancellationToken);
    }
}
