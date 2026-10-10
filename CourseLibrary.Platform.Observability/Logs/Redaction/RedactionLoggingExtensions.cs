using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;

public static class RedactionLoggingExtensions
{
    public static ILoggingBuilder AddCourseLibraryRedaction(
        this ILoggingBuilder loggingBuilder,
        Action<HmacRedactorOptions>? configureHmac = null)
    {
        ArgumentNullException.ThrowIfNull(loggingBuilder);

        loggingBuilder.EnableRedaction();

        if (configureHmac is not null)
        {
            loggingBuilder.Services.Configure(configureHmac);
        }

        loggingBuilder.Services.AddRedaction(options =>
        {
            options.SetRedactor<ErasingRedactor>(DataClassifications.Secret);
            options.SetRedactor<EmailRedactor>(DataClassifications.Email);
            options.SetRedactor<PhoneRedactor>(DataClassifications.Phone);
            options.SetRedactor<CreditCardRedactor>(DataClassifications.CreditCard);
            options.SetRedactor<PartialMaskingRedactor>(DataClassifications.PersonalData);
            options.SetRedactor<HmacRedactor>(DataClassifications.Sensitive);
        });

        return loggingBuilder;
    }
}