using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Infrastructure.Configuration.DataProtection;

public sealed class DataProtectionOptions
{
    public const string SectionName = "DataProtection";

    [Required]
    public string ApplicationName { get; init; } = "CourseLibrary";

    [Range(1, 3650)]
    public int KeyLifetimeDays { get; init; } = 90;

    [Required]
    public string KeyStorage { get; init; } = "AzureBlob";

    [Required]
    public string KeyBlobName { get; init; } =
        "DataProtection-Keys.xml";

    [Required]
    public string KeyContainerName { get; init; } =
        "dataprotection";
}