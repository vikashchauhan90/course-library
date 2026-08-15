namespace CourseLibrary.Gateway.Configuration.Authentication;

internal sealed class GatewayJwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Authority { get; set; } = string.Empty;

    public string? MetadataAddress { get; set; }

    public string Audience { get; set; } = string.Empty;

    public bool RequireHttpsMetadata { get; set; } = true;

    public bool ValidateIssuer { get; set; } = true;

    public bool ValidateAudience { get; set; } = true;

    public bool ValidateLifetime { get; set; } = true;

    public bool ValidateIssuerSigningKey { get; set; } = true;
}
