using CourseLibrary.Gateway.Configuration.Observability.Logs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using GatewayObservability = CourseLibrary.Gateway.Configuration.Observability;
using InfrastructureObservability = CourseLibrary.Infrastructure.Observability;

namespace CourseLibrary.Gateway.Configuration.Observability;


internal static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault();
        ConfigureResource(resourceBuilder, builder.Environment);

        // Add Logging with preprocessing for redaction and enrichment
        builder.AddLoggingObservability();
        builder.Services.TryAddSingleton<InfrastructureObservability.Traces.Processors.ExceptionActivityProcessor>();
        builder.Services.TryAddSingleton<InfrastructureObservability.Traces.Processors.CorrelationActivityProcessor>();
        builder.Services.TryAddSingleton<InfrastructureObservability.Traces.Processors.EnvironmentActivityProcessor>();
        builder.Services.TryAddSingleton<GatewayObservability.Traces.Processors.ApplicationActivityProcessor>();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options.AddProcessor(sp =>
            sp.GetRequiredService<InfrastructureObservability.Logs.Processors.CourseLibraryLogProcessor>());
        });

        builder.Services.AddOpenTelemetry()
               .ConfigureResource(resource =>
               {
                   ConfigureResource(resource, builder.Environment);
               })
             .WithTracing(tracing =>
             {
                 tracing
                     .AddSource(InfrastructureObservability.Traces.ActivitySources.Infrastructure.Name)
                     .AddSource(GatewayObservability.Traces.ActivitySources.Gateway.Name)

                     .AddAspNetCoreInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddHttpClientInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddSqlClientInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddEntityFrameworkCoreInstrumentation()
                     .AddProcessor<InfrastructureObservability.Traces.Processors.ExceptionActivityProcessor>()
                     .AddProcessor<InfrastructureObservability.Traces.Processors.CorrelationActivityProcessor>()
                     .AddProcessor<InfrastructureObservability.Traces.Processors.EnvironmentActivityProcessor>()
                     .AddProcessor<GatewayObservability.Traces.Processors.ApplicationActivityProcessor>();
             })
             .WithMetrics(metrics =>
             {
                 metrics
                     .AddMeter(InfrastructureObservability.Metrics.Meters.Infrastructure.Name)
                     .AddMeter(GatewayObservability.Metrics.Meters.Api.Name)
                     .AddMeter("Microsoft.AspNetCore.Hosting")
                     .AddMeter("Microsoft.AspNetCore*")
                     .AddMeter("CourseLibrary*")
                     .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                     .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")

                     .AddAspNetCoreInstrumentation()
                     .AddHttpClientInstrumentation()
                     .AddRuntimeInstrumentation()
                     .AddSqlClientInstrumentation()
                     .AddProcessInstrumentation();

             })
             .UseOtlpExporter();

        return builder;
    }

    private static void ConfigureResource(
    ResourceBuilder resource,
    IWebHostEnvironment env)
    {
        resource
            .AddService(
            serviceName: ObservabilityConstants.ServiceName,
            serviceVersion: ObservabilityConstants.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    [GatewayObservability.Traces.Attributes.DeploymentEnvironment] = env.EnvironmentName,
                    [GatewayObservability.Traces.Attributes.ServiceInstanceId] = Environment.MachineName
                });
    }
}