using CourseLibrary.Application.Behaviors;
using CourseLibrary.Application.Operations.Authors.Create;
using MediatorForge;
using MediatorForge.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.EventConsumer.Configuration;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibraryApplication(this IServiceCollection services)
    {
        services.AddCqrs();

        // Register pipeline behaviors as open generics
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        services.AddTransient<IHandler<CreateAuthorAuditCommand, Unit>, CreateAuthorAuditHandler>();

        return services;
    }
}
