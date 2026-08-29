using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration.Exceptions;
using CourseLibrary.Api.Configuration.Observability;
using CourseLibrary.Api.Configuration.Observability.Metrics;
using CourseLibrary.Api.Configuration.OutputCache;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Api.Configuration;

internal static class CourseLibraryHostExtensions
{
    public static WebApplicationBuilder AddCourseLibraryServices(
        this WebApplicationBuilder builder)
    {
        // W3C distributed tracing format.
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        // Kestrel configuration.
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
        });

        // .NET framework services.
        builder.Services.AddOptions();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services
    .AddHealthChecks()
     .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: ["live"])
    .AddAzureCosmosDB(
        name: "cosmos",
        tags: ["ready"]);

        builder.Services.AddOpenApi();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter());

            options.SerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;
        });

        builder.Services.AddCarter();

        // HTTP logging.
        builder.Services.AddHttpLogging();

        // Observability.
        builder.AddObservability();

        // Infrastructure services.
        builder.Services.AddCourseLibraryInfrastructure(builder.Configuration);

        // Application services
        builder.Services.AddCourseLibraryApplication();

        return builder;
    }

    public static WebApplication UseCourseLibraryPipeline(
     this WebApplication app)
    {
        app.UseGlobalExceptionHandler();

        app.UseHttpsRedirection();

        app.UseRequestContext();

        app.UseUserContext();

        app.UseRequestMetrics();

        app.UseHttpLogging();

        // Response headers / observability
        app.UseResponseHeaders();

        // Client/proxy HTTP response caching
        app.UseResponseCaching();

        // Server-side Output Cache
        app.UseOutputCache();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // Health endpoints
        app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            })
            .AllowAnonymous()
            .CacheOutput(OutputCachePolicies.Default);

        app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            })
            .AllowAnonymous();

        app.MapCarter();
        return app;
    }
}