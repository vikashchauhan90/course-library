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
        // Never trust identity headers supplied by the caller. The gateway is
        // the only component that may create these headers for downstream APIs.
        context.Request.Headers.Remove("X-User-Id");
        context.Request.Headers.Remove("X-Client-Id");
        context.Request.Headers.Remove("X-Identity-Type");

        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var identityType = _tokenIdentityService.GetIdentityType(context.User);
            switch (identityType)
            {
                case TokenIdentityType.User:
                    var subject = _tokenIdentityService.GetSubjectId(context.User);
                    if (!string.IsNullOrWhiteSpace(subject))
                    {
                        context.Request.Headers["X-User-Id"] = subject;
                    }

                    context.Request.Headers["X-Identity-Type"] = "User";
                    break;

                case TokenIdentityType.M2M:
                    var clientId = _tokenIdentityService.GetClientId(context.User);
                    if (!string.IsNullOrWhiteSpace(clientId))
                    {
                        context.Request.Headers["X-Client-Id"] = clientId;
                    }

                    context.Request.Headers["X-Identity-Type"] = "M2M";
                    break;
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
