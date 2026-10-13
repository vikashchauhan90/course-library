namespace CourseLibrary.Core.Domain.Abstractions.ValueObjects;

public interface IStronglyTypedValue<out TValue>
    : IValueObject
{
    TValue Value { get; }
}
