using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace CourseLibrary.Gateway.Configuration.Authentication;

internal sealed class UserIdentityForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenIdentityService _tokenIdentityService;

    public UserIdentityForwardingMiddleware(
        RequestDelegate next,
        ITokenIdentityService tokenIdentityService)
    {
        _next = next;
        _tokenIdentityService = tokenIdentityService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var subject = context.User.FindFirst("sub")?.Value;
            var clientId = context.User.FindFirst("azp")?.Value
                ?? context.User.FindFirst("client_id")?.Value
                ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (!string.IsNullOrWhiteSpace(subject))
            {
                context.Request.Headers["X-User-Id"] = subject;
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                context.Request.Headers["X-Client-Id"] = clientId;
            }

            var identityType = _tokenIdentityService.GetIdentityType(context.User);
            if (identityType != TokenIdentityType.Unknown)
            {
                context.Request.Headers["X-Identity-Type"] = identityType.ToString();
            }
        }

        await _next(context);
    }
}

internal static class UserIdentityForwardingMiddlewareExtensions
{
    public static IApplicationBuilder UseUserIdentityForwarding(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserIdentityForwardingMiddleware>();
    }
}
