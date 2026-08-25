using CourseLibrary.EventConsumer.Configuration.Observability.Logs;
using CourseLibrary.EventConsumer.Configuration.Observability.Traces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using InfrastructureObservability =
    CourseLibrary.Infrastructure.Observability;

namespace CourseLibrary.EventConsumer.Configuration.Observability;

internal static class OpenTelemetryExtensions
{
    public static IHostApplicationBuilder AddObservability(
        this IHostApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault();

        ConfigureResource(
            resourceBuilder,
            builder.Environment.EnvironmentName);

        builder.AddLoggingObservability();

        builder.Services.TryAddSingleton<
            InfrastructureObservability.Traces.Processors.ExceptionActivityProcessor>();

        builder.Services.TryAddSingleton<
            InfrastructureObservability.Traces.Processors.EnvironmentActivityProcessor>();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;

            options.AddProcessor(sp =>
                sp.GetRequiredService<
                    InfrastructureObservability.Logs.Processors.CourseLibraryLogProcessor>());
        });

        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                ConfigureResource(
                    resource,
                    builder.Environment.EnvironmentName);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(
                        InfrastructureObservability.Traces.ActivitySources.Infrastructure.Name)

                    .AddSource(
                        ActivitySources.EventConsumer.Name)
                    .AddSource("Microsoft.DurableTask")

                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })

                    .AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })

                    .AddProcessor<
                        InfrastructureObservability.Traces.Processors.ExceptionActivityProcessor>()

                    .AddProcessor<
                        InfrastructureObservability.Traces.Processors.EnvironmentActivityProcessor>();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(
                        InfrastructureObservability.Metrics.Meters.Infrastructure.Name)

                    .AddMeter(
                        ActivitySources.EventConsumer.Name)
                      .AddMeter("Microsoft.DurableTask")
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();
            })
            .UseOtlpExporter();

        return builder;
    }

    private static void ConfigureResource(
        ResourceBuilder resource,
        string environmentName)
    {
        resource
            .AddService(
                serviceName: ObservabilityConstants.ServiceName,
                serviceVersion: ObservabilityConstants.ServiceVersion)
            .AddAttributes(
                new Dictionary<string, object>
                {
                    [
                        Attributes.DeploymentEnvironment
                    ] = environmentName,

                    [
                        Attributes.ServiceInstanceId
                    ] = Environment.MachineName
                });
    }
}