using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace CourseLibrary.Platform.AspNetCore.RequestContext;

internal sealed class HttpRequestContext(
        IHttpContextAccessor accessor) : IRequestContext
{

    private HttpContext? HttpContext =>
        accessor.HttpContext;

    public string? TraceId =>
        Activity.Current?.TraceId.ToString();

    public string? TraceParent =>
    HttpContext?.Request.Headers[HttpRequestContextConstants.TraceParentHeaderName].FirstOrDefault();

    public string? TraceState =>
        HttpContext?.Request.Headers[HttpRequestContextConstants.TraceStateHeaderName].FirstOrDefault();
    public string? CorrelationId =>
        HttpContext?.Request.Headers[HttpRequestContextConstants.CorrelationIdHeaderName]
            .FirstOrDefault();

    public string? UserId =>
        HttpContext?.User.FindFirst("sub")?.Value
        ?? HttpContext?.Request.Headers[HttpRequestContextConstants.UserIdHeaderName].FirstOrDefault();

    public string? TenantId =>
        HttpContext?.Request.Headers[HttpRequestContextConstants.TenantIdHeaderName].FirstOrDefault();

    public string? IdempotencyKey =>
        HttpContext?.Request.Headers[HttpRequestContextConstants.IdempotencyHeaderName]
            .FirstOrDefault();

}