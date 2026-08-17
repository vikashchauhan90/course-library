using CourseLibrary.Gateway.Configuration.Observability.Logs;
using CourseLibrary.Gateway.Configuration.Observability.Metrics;
using CourseLibrary.Gateway.Configuration.Observability.Traces;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
        });

        builder.Services.AddOpenTelemetry()
               .ConfigureResource(resource =>
               {
                   ConfigureResource(resource, builder.Environment);
               })
             .WithTracing(tracing =>
             {
                 tracing
                     .AddSource(ActivitySources.Gateway.Name)

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
                     });
             })
             .WithMetrics(metrics =>
             {
                 metrics
                     .AddMeter(Meters.Gateway.Name)
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
                    [Attributes.DeploymentEnvironment] = env.EnvironmentName,
                    [Attributes.ServiceInstanceId] = Environment.MachineName
                });
    }
}