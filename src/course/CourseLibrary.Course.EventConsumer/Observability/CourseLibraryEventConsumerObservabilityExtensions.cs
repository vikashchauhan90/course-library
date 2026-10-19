using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Observability.Logs.Processors;
using CourseLibrary.Platform.Observability.Metrics;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Observability.Traces.Processors;
using CourseLibrary.Platform.Persistence.CosmosDB.Observability;
using CourseLibrary.Platform.Serialization.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CourseLibrary.Course.EventConsumer.Observability;

public static class CourseLibraryEventConsumerObservabilityExtensions
{
    public static IHostApplicationBuilder AddObservability(
        this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault();
        ConfigureResourceBuilder(
            resourceBuilder,
            builder.Environment.EnvironmentName);


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
            .ConfigureResource(resourceBuilder =>
            {
                ConfigureResourceBuilder(
                    resourceBuilder,
                    builder.Environment.EnvironmentName);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(CourseEventConsumerTelemetry.ActivitySourceName)
                    .AddSource(MessagingTelemetry.ActivitySourceName)
                    .AddSource(CoursePersistenceTelemetry.ActivitySourceName)
                    .AddSource(CosmosTelemetry.ActivitySourceName)
                    .AddSource(SerializationTelemetry.ActivitySourceName)
                    .AddSource(PlatformActivitySource.ActivitySourceName)
                    .AddSource("Microsoft.DurableTask")
                       .AddHttpClientInstrumentation(options =>
                       {
                           options.RecordException = true;
                       })
                     .AddSqlClientInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddProcessor<CorrelationActivityProcessor>()
                     .AddProcessor<ActivityFailureProcessor>();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(CourseEventConsumerTelemetry.MeterName)
                    .AddMeter(MessagingTelemetry.MeterName)
                    .AddMeter(CoursePersistenceTelemetry.MeterName)
                    .AddMeter(CosmosTelemetry.MeterName)
                    .AddMeter(SerializationTelemetry.MeterName)
                    .AddMeter(PlatformMeter.MeterName)
                    .AddMeter("Microsoft.DurableTask")
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();
            })
            .UseOtlpExporter();

        return builder;
    }

    private static void ConfigureResourceBuilder(
        ResourceBuilder builder, string environmentName)
    {
        var serviceVersion = typeof(CourseEventConsumerTelemetry)
           .Assembly
           .GetName()
           .Version?
           .ToString() ?? "unknown";

        builder
            .AddService(
                serviceName: CourseEventConsumerTelemetry.ActivitySourceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment.name"] = environmentName
            });
    }
}
