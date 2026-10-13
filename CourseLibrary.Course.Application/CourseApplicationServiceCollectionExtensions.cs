using CourseLibrary.Course.Application.Operations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Course.Application;

public static class CourseApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseApplicationOperations(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IValidator<OperationValidationInput>, OperationValidationInputValidator>();
        services.AddScoped<ICourseOperations, CourseOperations>();
        services.AddScoped<ICommentOperations, CommentOperations>();
        services.AddScoped<IDiscussionOperations, DiscussionOperations>();

        return services;
    }
}
