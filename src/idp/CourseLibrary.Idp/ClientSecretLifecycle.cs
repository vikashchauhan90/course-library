using CourseLibrary.Idp.Domain.Entities;

namespace CourseLibrary.Idp;

internal static class ClientSecretLifecycle
{
    public static DateTimeOffset? GetEffectiveExpiry(
        OpenIddictApplication application,
        IConfiguration configuration)
    {
        if (string.IsNullOrEmpty(application.ClientSecret))
            return null;

        return application.SecretExpiresAt
            ?? GetExpiry(
                application.SecretRotatedAt ?? application.SecretCreatedAt ?? application.CreatedAt,
                configuration);
    }

    public static DateTimeOffset GetExpiry(
        DateTimeOffset start,
        IConfiguration configuration)
    {
        var lifetimeDays = Math.Clamp(
            configuration.GetValue("Security:ClientSecretLifetimeDays", 365),
            1,
            3650);
        return start.AddDays(lifetimeDays);
    }
}
