using Microsoft.Extensions.Compliance.Classification;

namespace CourseLibrary.Infrastructure.Observability.Logs.Redaction;

[AttributeUsage(
    AttributeTargets.Parameter |
    AttributeTargets.Property |
    AttributeTargets.Field)]
public sealed class PersonalDataAttribute : DataClassificationAttribute
{
    public PersonalDataAttribute()
        : base(DataClassifications.PersonalData)
    {
    }
}