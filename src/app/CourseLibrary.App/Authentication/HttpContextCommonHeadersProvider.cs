using CourseLibrary.Client.Observability;

namespace CourseLibrary.App.Authentication;

public sealed class HttpContextCommonHeadersProvider(
    IHttpContextAccessor httpContextAccessor) : ICommonHeadersProvider
{
    private static readonly string[] HeaderNames =
    [
        "X-Correlation-ID",
        "X-Request-ID",
        "Idempotency-Key",
        "traceparent",
        "tracestate",
        "baggage"
    ];

    public IReadOnlyDictionary<string, string> GetHeaders()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return new Dictionary<string, string>();
        }

        return HeaderNames
            .Where(request.Headers.ContainsKey)
            .ToDictionary(
                headerName => headerName,
                headerName => request.Headers[headerName].ToString(),
                StringComparer.OrdinalIgnoreCase);
    }
}