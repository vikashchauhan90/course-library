using CourseLibrary.Application.Abstractions.RequestContext;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace CourseLibrary.Infrastructure.RequestContext;

internal sealed class HttpRequestContext(
        IHttpContextAccessor accessor) : IRequestContext
{

    private HttpContext? HttpContext =>
        accessor.HttpContext;

    public string? TraceId =>
        Activity.Current?.TraceId.ToString();

    public string? TraceParent =>
    HttpContext?.Request.Headers["traceparent"].FirstOrDefault();
    public string? CorrelationId =>
        HttpContext?.Request.Headers["X-Correlation-Id"]
            .FirstOrDefault();

    public string? UserId =>
        HttpContext?.User.FindFirst("sub")?.Value;

    public string? ClientId =>
        HttpContext?.User.FindFirst("client_id")?.Value;

    public string? IdempotencyKey =>
        HttpContext?.Request.Headers["Idempotency-Key"]
            .FirstOrDefault();

    public bool IsAuthenticated =>
        HttpContext?.User.Identity?.IsAuthenticated == true;

}
