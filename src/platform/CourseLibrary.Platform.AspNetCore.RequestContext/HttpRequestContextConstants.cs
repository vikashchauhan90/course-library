
namespace CourseLibrary.Platform.AspNetCore.RequestContext;

public static class HttpRequestContextConstants
{
    public const string CorrelationIdHeaderName = "x-correlation-id";
    public const string TraceParentHeaderName = "traceparent";
    public const string TraceStateHeaderName = "tracestate";
    public const string IdempotencyHeaderName = "x-idempotency-key";
    public const string UserIdHeaderName = "x-user-id";
    public const string TenantIdHeaderName = "x-tenant-id";

}
