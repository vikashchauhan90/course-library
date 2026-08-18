namespace CourseLibrary.Idp;

public sealed class OpenIdOptions
{
    public const string SectionName = "OpenId";
    public string Issuer { get; set; } = string.Empty;
    public string ApiScope { get; set; } = "course-library-api";
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 14;
    public string? SigningCertificatePath { get; set; }
    public string? SigningCertificatePassword { get; set; }
    public string? EncryptionCertificatePath { get; set; }
    public string? EncryptionCertificatePassword { get; set; }
}
