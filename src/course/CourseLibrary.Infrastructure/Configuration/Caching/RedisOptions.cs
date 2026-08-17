using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Infrastructure.Configuration.Caching;

public sealed class RedisOptions : IValidatableObject
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }

    public string InstanceName { get; init; } = "CourseLibrary:";

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            yield return new ValidationResult(
                $"{nameof(ConnectionString)} is required.",
                [nameof(ConnectionString)]);
        }

        if (string.IsNullOrWhiteSpace(InstanceName))
        {
            yield return new ValidationResult(
                $"{nameof(InstanceName)} is required.",
                [nameof(InstanceName)]);
        }
    }
}