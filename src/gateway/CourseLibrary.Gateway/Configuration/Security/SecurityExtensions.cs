using CourseLibrary.Gateway.Configuration.Security.Middlewares;

namespace CourseLibrary.Gateway.Configuration.Security;

public static class SecurityExtensions
{
    public static IApplicationBuilder UseResponseHeaderCleanup(this IApplicationBuilder app) 
    { 
        return app.UseMiddleware<ResponseHeaderCleanupMiddleware>();
    }
}
