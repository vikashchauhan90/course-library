using CourseLibrary.App.Configuration.Observability.Logs;
using CourseLibrary.App.Configuration.Observability.Metrics;
using CourseLibrary.App.Configuration.Observability.Traces;
using CourseLibrary.App.Configuration.Observability.Traces.Middlewares;
using CourseLibrary.App.Configuration.Observability.Traces.Processors;
using CourseLibrary.App.Observability.Logs.Processors;
using CourseLibrary.Client.Observability;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CourseLibrary.App.Configuration.Observability;


internal static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddObservability(
        this WebApplicationBuilder builder)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault();
        ConfigureResource(resourceBuilder, builder.Environment);

        // Add Logging with preprocessing for redaction and enrichment
        builder.AddLoggingObservability();
        builder.Services.TryAddSingleton<ApplicationActivityProcessor>();

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
               {
                   ConfigureResource(resource, builder.Environment);
               })
             .WithTracing(tracing =>
             {
                 tracing
                     .AddSource(ActivitySources.App.Name)
                     .AddSource(CourseApiDiagnostics.ActivitySourceName)
                     .AddAspNetCoreInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddHttpClientInstrumentation(options =>
                     {
                         options.RecordException = true;
                     })
                     .AddProcessor<ApplicationActivityProcessor>();
             })
             .WithMetrics(metrics =>
             {
                 metrics
                     .AddMeter(Meters.App.Name)
                     .AddMeter(CourseApiDiagnostics.MeterName)
                     .AddMeter("Microsoft.AspNetCore.Hosting")
                     .AddMeter("Microsoft.AspNetCore*")
                     .AddMeter("CourseLibrary*")
                     .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                     .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")

                     .AddAspNetCoreInstrumentation()
                     .AddHttpClientInstrumentation()
                     .AddRuntimeInstrumentation();

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
                    [Traces.Attributes.DeploymentEnvironment] = env.EnvironmentName,
                    [Traces.Attributes.ServiceInstanceId] = Environment.MachineName
                });
    }

    public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestContextMiddleware>();
    }
}