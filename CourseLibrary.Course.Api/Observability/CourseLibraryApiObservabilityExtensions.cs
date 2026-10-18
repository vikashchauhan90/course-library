using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Platform.Caching.Observability;
using CourseLibrary.Platform.DataProtection.Observability;
using CourseLibrary.Platform.Idempotency.Observability;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Observability;
using CourseLibrary.Platform.Observability.Metrics;
using CourseLibrary.Platform.Observability.Logs.Processors;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Observability.Traces.Processors;
using CourseLibrary.Platform.Persistence.CosmosDB.Observability;
using CourseLibrary.Platform.Resilience.Observability;
using CourseLibrary.Platform.Serialization.Observability;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CourseLibrary.Course.Api.Observability;

public static class CourseLibraryApiObservabilityExtensions
{
    public static WebApplicationBuilder AddCourseLibraryApiObservability(
        this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var resourceBuilder = ResourceBuilder.CreateDefault();
        ConfigureResourceBuilder(resourceBuilder, builder.Environment.EnvironmentName);

        builder.Services.AddCourseLibraryObservabilityServices();
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
                    .AddSource(CourseApplicationTelemetry.ActivitySourceName)
                    .AddSource(CoursePersistenceTelemetry.ActivitySourceName)
                    .AddSource(ResilienceTelemetry.ActivitySourceName)
                    .AddSource(SerializationTelemetry.ActivitySourceName)
                    .AddSource(PlatformActivitySource.ActivitySourceName)
                    .AddSource(CacheTelemetry.ActivitySourceName)
                    .AddSource(DataProtectionTelemetry.ActivitySourceName)
                    .AddSource(IdempotencyTelemetry.ActivitySourceName)
                    .AddSource(MessagingTelemetry.ActivitySourceName)
                    .AddSource(CosmosTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation(options => options.RecordException = true)
                    .AddSqlClientInstrumentation(options => options.RecordException = true)
                    .AddProcessor<CorrelationActivityProcessor>()
                    .AddProcessor<ActivityFailureProcessor>();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(CourseApplicationTelemetry.MeterName)
                    .AddMeter(CoursePersistenceTelemetry.MeterName)
                    .AddMeter(ResilienceTelemetry.MeterName)
                    .AddMeter(SerializationTelemetry.MeterName)
                    .AddMeter(CacheTelemetry.MeterName)
                    .AddMeter(DataProtectionTelemetry.MeterName)
                    .AddMeter(IdempotencyTelemetry.MeterName)
                    .AddMeter(MessagingTelemetry.MeterName)
                    .AddMeter(CosmosTelemetry.MeterName)
                    .AddMeter(PlatformMeter.MeterName)
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
                serviceName: "CourseLibrary.Course.Api",
                serviceVersion: typeof(CourseLibraryApiObservabilityExtensions)
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
