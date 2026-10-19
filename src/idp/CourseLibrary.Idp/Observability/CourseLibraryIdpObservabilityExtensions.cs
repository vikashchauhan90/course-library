using CourseLibrary.Platform.Observability;
using CourseLibrary.Platform.Observability.Logs.Processors;
using CourseLibrary.Platform.Observability.Metrics;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Observability.Traces.Processors;
using CourseLibrary.Platform.Persistence.Sql.Observability;
using CourseLibrary.Platform.Resilience.Observability;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CourseLibrary.Idp.Observability;

internal static class CourseLibraryIdpObservabilityExtensions
{
    public static WebApplicationBuilder AddIdpObservability(
        this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var resourceBuilder = ResourceBuilder.CreateDefault();
        ConfigureResourceBuilder(resourceBuilder, builder.Environment.EnvironmentName);

        builder.Services.AddCourseLibraryObservabilityServices();
        builder.Logging.EnableCourseLibraryRedaction();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options.AddProcessor(sp =>
                sp.GetRequiredService<CourseLibraryLogProcessor>());
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                ConfigureResourceBuilder(resource, builder.Environment.EnvironmentName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(IdpTelemetry.ActivitySourceName)
                    .AddSource(ResilienceTelemetry.ActivitySourceName)
                    .AddSource(PlatformActivitySource.ActivitySourceName)
                    .AddSource(SqlTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation(options => options.RecordException = true)
                    .AddProcessor<CorrelationActivityProcessor>()
                    .AddProcessor<ActivityFailureProcessor>();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(IdpTelemetry.MeterName)
                    .AddMeter(ResilienceTelemetry.MeterName)
                    .AddMeter(PlatformMeter.MeterName)
                    .AddMeter(SqlTelemetry.MeterName)
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();
            })
            .UseOtlpExporter();

        return builder;
    }

    private static void ConfigureResourceBuilder(
        ResourceBuilder builder,
        string environmentName)
    {
        builder
            .AddService(
                serviceName: IdpTelemetry.ActivitySourceName,
                serviceVersion: typeof(CourseLibraryIdpObservabilityExtensions)
                    .Assembly
                    .GetName()
                    .Version?
                    .ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment.name"] = environmentName
            });
    }
}
