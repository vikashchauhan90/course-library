using Carter;
using CourseLibrary.Api.Configuration.Observability;
using CourseLibrary.Api.Configuration.Observability.Logs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using System.Diagnostics;
using CourseLibrary.Infrastructure.Resilience;

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

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddCors();
        builder.Services.AddHealthChecks();

        // HTTP logging.
        builder.Services.AddHttpLogging();

        // Application services.
        builder.Services.AddCarter();

        // Observability.
        builder.AddObservability();

        builder.Services.AddCourseLibraryHttpResilience(builder.Configuration);
        builder.Services.AddCourseLibraryResilience();
        builder.Services.AddSingleton<PolicyFactory>();

        return builder;
    }

    public static WebApplication UseCourseLibraryPipeline(
        this WebApplication app)
    {
        // Transport.
        app.UseHttpsRedirection();

        // Request context / observability.
        app.UseRequestContext();
        app.UseHttpLogging();

        // Security.
        app.UseAuthentication();
        app.UseUserContext();
        app.UseAuthorization();

        // Application endpoints.
        app.MapCarter();

        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapHealthChecks("/health/ready").AllowAnonymous();

        return app;
    }
}