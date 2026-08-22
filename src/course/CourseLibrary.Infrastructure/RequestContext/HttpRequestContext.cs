using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Infrastructure.Configuration.Idempotency;
using CourseLibrary.Infrastructure.Observability.Traces;
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
    HttpContext?.Request.Headers[TraceHeaders.TraceParent].FirstOrDefault();

    public string? TraceState =>
        HttpContext?.Request.Headers[TraceHeaders.TraceState].FirstOrDefault();
    public string? CorrelationId =>
        HttpContext?.Request.Headers[TraceHeaders.CorrelationId]
            .FirstOrDefault();

    public string? UserId =>
        HttpContext?.User.FindFirst("sub")?.Value
        ?? HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();

    public string? ClientId =>
        HttpContext?.User.FindFirst("client_id")?.Value
        ?? HttpContext?.Request.Headers["X-Client-Id"].FirstOrDefault();

    public string? IdempotencyKey =>
        HttpContext?.Request.Headers[IdempotencyHeader.HeaderName]
            .FirstOrDefault();

    public bool IsAuthenticated =>
        HttpContext?.User.Identity?.IsAuthenticated == true
        || HttpContext?.Request.Headers["X-Identity-Type"].FirstOrDefault()
            is "User" or "M2M";

}
