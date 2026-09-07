using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Client.Configuration;

public sealed class CourseLibraryClientOptions : IValidatableObject
{
    public const string SectionName = "CourseLibraryClient";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps)
        {
            yield return new ValidationResult("The base URL must be an absolute HTTPS URI.", new[] { nameof(BaseUrl) });
        }

        if (TimeoutSeconds <= 0)
        {
            yield return new ValidationResult("The timeout must be a positive integer.", new[] { nameof(TimeoutSeconds) });
        }
    }
}
