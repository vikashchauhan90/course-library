using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Carter;
using CourseLibrary.Api.Configuration.Exceptions;
using CourseLibrary.Api.Configuration.Observability;
using CourseLibrary.Api.Configuration.Observability.Metrics;
using CourseLibrary.Api.Configuration.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore;
using System.Diagnostics;
using CourseLibrary.Application.Configuration;
using CourseLibrary.Infrastructure.Configuration;

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

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CourseLibrary API",
                Version = "v1",
                Description = "Default CourseLibrary API version"
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "CourseLibrary API",
                Version = "v2",
                Description = "Additional CourseLibrary API version"
            });
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

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"CourseLibrary API {description.GroupName.ToUpperInvariant()}");
            }
        });

        app.MapCarter();
        app.MapHealthChecks("/health/live").AllowAnonymous();
        app.MapHealthChecks("/health/ready").AllowAnonymous();

        return app;
    }
}