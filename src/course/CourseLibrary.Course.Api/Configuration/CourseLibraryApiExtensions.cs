using Asp.Versioning;
using Carter;
using CourseLibrary.Course.Api.Middlewares;
using CourseLibrary.Course.Api.Observability;
using CourseLibrary.Course.Application;
using CourseLibrary.Course.Persistence;
using CourseLibrary.Platform.AspNetCore.RequestContext;
using CourseLibrary.Platform.Messaging;
using CourseLibrary.Platform.Resilience;
using CourseLibrary.Platform.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseLibrary.Course.Api.Configuration;

public static class CourseLibraryApiExtensions
{
    public static WebApplicationBuilder AddCourseLibraryApi(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;
        builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

        builder.Services.AddOptions();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient();
        builder.Services.AddOpenApi();
        builder.Services.AddCarter();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpLogging();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddAzureCosmosDB(name: "cosmos", tags: ["ready"]);

        builder.Services.AddCourseLibraryRequestContext();
        builder.Services.AddCourseLibrarySerializers();
        builder.Services.AddCourseLibraryServiceBus(builder.Configuration);
        builder.Services.AddCourseLibraryResilience();
        builder.Services.AddCosmosDatabase(builder.Configuration);
        builder.Services.AddRepositories();
        builder.Services.AddCourseApplicationOperations();
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            options.Level = CompressionLevel.SmallestSize);

        builder.Services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });
        builder.AddApiObservability();
        return builder;
    }

    public static WebApplication UseCourseLibraryApi(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseCourseApiTelemetry();
        app.UseGlobalExceptionHandler();
        app.UseHttpsRedirection();
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }
        app.UseRequestContext();

        app.UseUserContext();

        app.UseRequestMetrics();

        app.UseHttpLogging();

        // Response compression
        app.UseResponseCompression();
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

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        }).AllowAnonymous();
        app.MapCarter();

        return app;
    }

}
