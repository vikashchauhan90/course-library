
namespace CourseLibrary.Core.Domain.Abstractions.ValueObjects;

public static class StronglyTypedValueExtensions
{
    public static TValue Unwrap<TValue>(
       this IStronglyTypedValue<TValue> value)
    {
        return value.Value;
    }
}
