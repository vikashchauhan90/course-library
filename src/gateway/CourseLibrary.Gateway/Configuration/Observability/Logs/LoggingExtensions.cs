using CourseLibrary.Gateway.Configuration.Observability.Logs.Middlewares;
using CourseLibrary.Infrastructure.Observability.Logs.Processors;
using CourseLibrary.Infrastructure.Observability.Logs.Redaction;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseLibrary.Gateway.Configuration.Observability.Logs;

internal static class LoggingExtensions
{
    public static WebApplicationBuilder AddLoggingObservability(
       this WebApplicationBuilder builder)
    {
        builder.Logging.EnableRedaction();
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

    public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestContextMiddleware>();
    }

    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserContextMiddleware>();
    }
}