using CourseLibrary.Application.Behaviors;
using CourseLibrary.Application.Operations.Courses.Create;
using FluentValidation;
using MediatorForge;
using MediatorForge.Abstractions;

namespace CourseLibrary.Api.Configuration.Application;

internal static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibraryApplication(this IServiceCollection services)
    {
        services.AddCqrs();

        // Register handlers and pipeline behaviors from this assembly
        services.AddHandlersFromAssemblyContaining<CreateCourseCommandHandler>();

        // Register pipeline behaviors as open generics
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssemblyContaining<CreateCourseValidator>(includeInternalTypes: true);

        return services;
    }
}
