using CourseLibrary.Course.Application.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Course.Application;

public static class CourseApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseApplicationOperations(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<CourseOperations>();
        services.AddScoped<CommentOperations>();
        services.AddScoped<DiscussionOperations>();

        return services;
    }
}
