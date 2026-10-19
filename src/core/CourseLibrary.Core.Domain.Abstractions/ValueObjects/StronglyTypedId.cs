
namespace CourseLibrary.Core.Domain.Abstractions.ValueObjects;

public abstract record StronglyTypedId<T>(T Value)
    : IStronglyTypedValue<T>
    where T : notnull
{
    public override string ToString() =>
        Value.ToString() ?? string.Empty;
}