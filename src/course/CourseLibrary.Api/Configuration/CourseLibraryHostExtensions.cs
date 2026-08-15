using Carter;
using CourseLibrary.Api.Configuration.Caching;
using CourseLibrary.Api.Configuration.Exceptions;
using CourseLibrary.Api.Configuration.Idempotency;
using CourseLibrary.Api.Configuration.Observability;
using CourseLibrary.Api.Configuration.Observability.Metrics;
using CourseLibrary.Api.Configuration.Security;
using CourseLibrary.Api.Configuration.Serializers;
using CourseLibrary.Infrastructure.Resilience;
using System.Diagnostics;
using CourseLibrary.Application.Configuration;

namespace CourseLibrary.Api.Configuration;

internal static class CourseLibraryHostExtensions
{
    public static WebApplicationBuilder AddCourseLibraryServices(
        this WebApplicationBuilder builder)
    {
        // W3C distributed tracing format.
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        // .NET framework services.
        builder.Services.AddOptions();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();

        // HTTP logging.
        builder.Services.AddHttpLogging();

        // Application services.
        builder.Services.AddCarter();

        builder.Services.AddCourseLibrarySerializers();
        builder.Services.AddCourseLibraryMemoryCache();
        builder.Services.AddCourseLibraryIdempotency();

        // Observability.
        builder.AddObservability();

        builder.Services.AddCourseLibraryHttpResilience(builder.Configuration);
        builder.Services.AddCourseLibraryResilience();
        builder.Services.AddSingleton<PolicyFactory>();

        // Application CQRS and behaviors
        builder.Services.AddCourseLibraryApplication();

        return builder;
    }

    public static WebApplication UseCourseLibraryPipeline(
        this WebApplication app)
    {
        app.UseRequestMetrics();
        app.UseSecurityHeaders();
        app.UseGlobalExceptionHandler();
        app.UseHttpsRedirection();
        app.UseRequestContext();
        app.UseHttpLogging();
        app.UseUserContext();
        app.MapCarter();
        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapHealthChecks("/health/ready").AllowAnonymous();

        return app;
    }
}