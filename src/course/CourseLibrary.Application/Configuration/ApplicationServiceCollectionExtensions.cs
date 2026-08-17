using CourseLibrary.Application.Behaviors;
using CourseLibrary.Application.Operations.Authors.Create;
using CourseLibrary.Application.Operations.Comments.Create;
using CourseLibrary.Application.Operations.Courses.Create;
using CourseLibrary.Application.Operations.Discussions.Create;
using FluentValidation;
using MediatorForge;
using MediatorForge.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CourseLibrary.Application.Configuration;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddCourseLibraryApplication(this IServiceCollection services)
    {
        services.AddCqrs();

        // Register handlers and pipeline behaviors from the application assembly
        services.AddHandlersFromAssemblyContaining<CreateCourseCommandHandler>();

        // Register pipeline behaviors as open generics
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        // Register FluentValidation validators from the application assembly
        services.AddValidatorsFromAssemblyContaining<CreateCourseValidator>(includeInternalTypes: true);
        services.AddValidatorsFromAssemblyContaining<CreateAuthorValidator>(includeInternalTypes: true);
        services.AddValidatorsFromAssemblyContaining<CreateCommentValidator>(includeInternalTypes: true);
        services.AddValidatorsFromAssemblyContaining<CreateDiscussionValidator>(includeInternalTypes: true);

        // Register event handlers from the application assembly
        services.AddEventHandlersFromAssemblyContaining<CourseCreatedEventHandler>();

        return services;
    }
}
