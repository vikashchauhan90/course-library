using Microsoft.Extensions.Compliance.Classification;

namespace CourseLibrary.Course.App.Observability.Logs.Redaction;

[AttributeUsage(
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Field)]
public sealed class SensitiveAttribute : DataClassificationAttribute
{
    public SensitiveAttribute()
        : base(DataClassifications.Sensitive)
    {
    }
}
