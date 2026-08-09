using Carter;
using CourseLibrary.Api.Configuration.Caching;
using CourseLibrary.Api.Configuration.Exceptions;
using CourseLibrary.Api.Configuration.Filters;
using CourseLibrary.Api.Configuration.Idempotency;
using CourseLibrary.Api.Configuration.Observability;
using CourseLibrary.Api.Configuration.Observability.Logs;
using CourseLibrary.Api.Configuration.Observability.Logs.Middlewares;
using CourseLibrary.Application.Abstractions.Serialization;
using CourseLibrary.Application.Abstractions.Serializers;
using CourseLibrary.Infrastructure;
using CourseLibrary.Infrastructure.Caching;
using CourseLibrary.Infrastructure.Cosmos;
using CourseLibrary.Infrastructure.Idempotency;
using CourseLibrary.Infrastructure.Resilience;
using CourseLibrary.Infrastructure.Serializers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using System.Diagnostics;
using System.Text.Json;

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

        builder.Services.AddCourseLibraryMemoryCache();
        builder.Services.AddCourseLibraryIdempotency();
        builder.Services.AddCourseLibraryCosmosRepositories(builder.Configuration);

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
        app.UseGlobalExceptionHandler();
        // Transport.
        app.UseHttpsRedirection();

        // Request context / observability.
        app.UseRequestContext();
        app.UseHttpLogging();

        // Security.
        app.UseAuthentication();
        app.UseMiddleware<HeaderUserContextMiddleware>();
        app.UseUserContext();
        app.UseAuthorization();

        // Application endpoints.
        app.MapCarter();

        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapHealthChecks("/health/ready").AllowAnonymous();

        return app;
    }
}