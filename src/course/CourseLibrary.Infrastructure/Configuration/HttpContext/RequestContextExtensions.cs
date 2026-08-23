using CourseLibrary.Application.Abstractions.RequestContext;
using CourseLibrary.Infrastructure.RequestContext;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Infrastructure.Configuration.HttpContext;

public static class RequestContextExtensions
{
    public static IServiceCollection AddCourseLibraryRequestContext(
        this IServiceCollection services)
    {
        services.AddScoped<IRequestContext, HttpRequestContext>();
        return services;
    }
}
