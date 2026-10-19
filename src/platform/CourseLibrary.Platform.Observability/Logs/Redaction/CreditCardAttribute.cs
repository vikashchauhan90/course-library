using Microsoft.Extensions.Compliance.Classification;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;

[AttributeUsage(
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Field)]
public sealed class CreditCardAttribute : DataClassificationAttribute
{
    public CreditCardAttribute()
        : base(DataClassifications.CreditCard)
    {
    }
}