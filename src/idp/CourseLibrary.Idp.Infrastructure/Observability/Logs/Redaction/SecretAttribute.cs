using Microsoft.Extensions.Compliance.Classification;

namespace CourseLibrary.Idp.Infrastructure.Observability.Logs.Redaction;

[AttributeUsage(
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Field)]
public sealed class SecretAttribute : DataClassificationAttribute
{
    public SecretAttribute()
        : base(DataClassifications.Secret)
    {
    }
}
