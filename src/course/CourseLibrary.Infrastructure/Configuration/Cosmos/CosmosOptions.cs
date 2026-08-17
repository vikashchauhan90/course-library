using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Infrastructure.Configuration.Cosmos;

public sealed class CosmosOptions : IValidatableObject
{
    public const string SectionName = "Cosmos";

    public required string AccountEndpoint { get; init; }

    public required string AccountKey { get; init; }

    public required string DatabaseName { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(AccountEndpoint))
        {
            yield return new ValidationResult(
                $"{nameof(AccountEndpoint)} is required.",
                [nameof(AccountEndpoint)]);
        }

        if (string.IsNullOrWhiteSpace(AccountKey))
        {
            yield return new ValidationResult(
                $"{nameof(AccountKey)} is required.",
                [nameof(AccountKey)]);
        }

        if (string.IsNullOrWhiteSpace(DatabaseName))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseName)} is required.",
                [nameof(DatabaseName)]);
        }
    }
}