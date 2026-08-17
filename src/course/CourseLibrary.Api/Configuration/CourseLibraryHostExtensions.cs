using Asp.Versioning;
using Carter;
using CourseLibrary.Api.Configuration.Exceptions;
using CourseLibrary.Api.Configuration.Observability;
using CourseLibrary.Api.Configuration.Observability.Metrics;
using CourseLibrary.Api.Configuration.Security;
using CourseLibrary.Application.Configuration;
using CourseLibrary.Infrastructure.Configuration;
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

        // .NET framework services.
        builder.Services.AddOptions();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks();
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
        app.UseRequestMetrics();
        app.UseSecurityHeaders();
        app.UseGlobalExceptionHandler();
        app.UseHttpsRedirection();
        app.UseRequestContext();
        app.UseHttpLogging();
        app.UseUserContext();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        

        app.MapCarter();
        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapHealthChecks("/health/ready").AllowAnonymous();

        return app;
    }
}