using CourseLibrary.Api.Configuration.Security.Middlewares;

namespace CourseLibrary.Api.Configuration.Security;

public static class SecurityExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
