using Microsoft.Extensions.Compliance.Classification;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;

[AttributeUsage(
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Field)]
public sealed class PhoneAttribute : DataClassificationAttribute
{
    public PhoneAttribute()
        : base(DataClassifications.Phone)
    {
    }
}