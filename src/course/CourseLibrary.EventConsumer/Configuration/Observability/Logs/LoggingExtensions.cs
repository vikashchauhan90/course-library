
using CourseLibrary.Infrastructure.Observability.Logs.Processors;
using CourseLibrary.Infrastructure.Observability.Logs.Redaction;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.EventConsumer.Configuration.Observability.Logs;

internal static class LoggingExtensions
{
    public static IHostApplicationBuilder AddLoggingObservability(
       this IHostApplicationBuilder builder)
    {
        builder.Logging.EnableRedaction();
        builder.Logging.EnableEnrichment();
        builder.Services.TryAddSingleton<CourseLibraryLogProcessor>();
        builder.Services.AddRedaction(options =>
        {
            // Passwords, tokens, secrets
            options.SetRedactor<ErasingRedactor>(
                DataClassifications.Secret);


            // Email masking
            options.SetRedactor<EmailRedactor>(
                DataClassifications.Email);


            // General personal information
            // Example:
            // name, customer reference, identifiers
            options.SetRedactor<PartialMaskingRedactor>(
                DataClassifications.PersonalData);


            // Values where correlation is useful
            // Example:
            // tenant id, external reference id
            options.SetRedactor<HmacRedactor>(
                DataClassifications.Sensitive);

        });


        return builder;
    }
}