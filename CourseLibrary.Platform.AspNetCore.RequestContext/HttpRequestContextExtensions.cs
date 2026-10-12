using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Platform.AspNetCore.RequestContext;

public static class HttpRequestContextExtensions
{
    public static IServiceCollection AddCourseLibraryRequestContext(
        this IServiceCollection services)
    {
        services.AddScoped<IRequestContext, HttpRequestContext>();
        return services;
    }
}
