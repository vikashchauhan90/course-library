using CourseLibrary.Course.Application.Observability;
using CourseLibrary.Course.EventConsumer.Observability;
using CourseLibrary.Course.Persistence.Observability;
using CourseLibrary.Platform.Messaging.Observability;
using CourseLibrary.Platform.Persistence.CosmosDB.Observability;
using CourseLibrary.Platform.Resilience.Observability;
using CourseLibrary.Platform.Serialization.Observability;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CourseLibrary.Course.EventConsumer.Configuration.Observability;

internal static class CourseLibraryEventConsumerObservabilityExtensions
{
    public static IHostApplicationBuilder AddCourseLibraryEventConsumerObservability(
        this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: "CourseLibrary.Course.EventConsumer",
                serviceVersion: typeof(CourseLibraryEventConsumerObservabilityExtensions)
                    .Assembly.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(CourseEventConsumerTelemetry.ActivitySourceName)
                    .AddSource(CourseApplicationTelemetry.ActivitySourceName)
                    .AddSource(CoursePersistenceTelemetry.ActivitySourceName)
                    .AddSource(MessagingTelemetry.ActivitySourceName)
                    .AddSource(CosmosTelemetry.ActivitySourceName)
                    .AddSource(ResilienceTelemetry.ActivitySourceName)
                    .AddSource(SerializationTelemetry.ActivitySourceName)
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
                    .AddMeter(CourseApplicationTelemetry.MeterName)
                    .AddMeter(CoursePersistenceTelemetry.MeterName)
                    .AddMeter(MessagingTelemetry.MeterName)
                    .AddMeter(CosmosTelemetry.MeterName)
                    .AddMeter(ResilienceTelemetry.MeterName)
                    .AddMeter(SerializationTelemetry.MeterName)
                    .AddMeter("Microsoft.DurableTask")
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation();

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
