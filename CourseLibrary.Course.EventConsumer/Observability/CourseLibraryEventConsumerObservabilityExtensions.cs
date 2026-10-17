using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Observability.Metrics;
using CourseLibrary.Platform.Observability.Traces;
using CourseLibrary.Platform.Persistence.CosmosDB.Observability;
using CourseLibrary.Platform.Serialization.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CourseLibrary.Course.EventConsumer.Observability;

public static class CourseLibraryEventConsumerObservabilityExtensions
{
    public static IHostApplicationBuilder AddCourseLibraryEventConsumerObservability(
        this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var serviceVersion = typeof(CourseEventConsumerTelemetry)
            .Assembly
            .GetName()
            .Version?
            .ToString() ?? "unknown";

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: CourseEventConsumerTelemetry.ActivitySourceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment.name"] = builder.Environment.EnvironmentName
            });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;

            if (builder.Environment.IsDevelopment())
            {
                options.AddConsoleExporter();
            }
            else
            {
                options.AddOtlpExporter();
            }
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(
                        serviceName: CourseEventConsumerTelemetry.ActivitySourceName,
                        serviceVersion: serviceVersion,
                        serviceInstanceId: Environment.MachineName)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment.name"] = builder.Environment.EnvironmentName
                    });
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
                    .AddHttpClientInstrumentation(options => options.RecordException = true);

                if (builder.Environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }
                else
                {
                    tracing.AddOtlpExporter();
                }
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

                if (builder.Environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }
                else
                {
                    metrics.AddOtlpExporter();
                }
            });

        return builder;
    }
}
