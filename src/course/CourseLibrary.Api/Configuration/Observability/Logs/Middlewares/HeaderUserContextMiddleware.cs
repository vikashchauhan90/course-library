using System.Security.Claims;

namespace CourseLibrary.Api.Configuration.Observability.Logs.Middlewares;

internal sealed class HeaderUserContextMiddleware
{
    private readonly RequestDelegate _next;

    public HeaderUserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            var userId = context.Request.Headers["X-User-Id"].ToString();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("sub", userId)
                };

                var clientId = context.Request.Headers["X-Client-Id"].ToString();
                if (!string.IsNullOrWhiteSpace(clientId))
                {
                    claims.Add(new Claim("client_id", clientId));
                }

                var identityType = context.Request.Headers["X-Identity-Type"].ToString();
                if (!string.IsNullOrWhiteSpace(identityType))
                {
                    claims.Add(new Claim("identity_type", identityType));
                }

                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(claims, "HeaderAuthentication"));
            }
        }

        await _next(context);
    }
}
