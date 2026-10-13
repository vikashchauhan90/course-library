namespace CourseLibrary.Core.Domain.Abstractions.ValueObjects;

public abstract class ValueObject: IValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other &&
               GetEqualityComponents()
                   .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                0,
                HashCode.Combine);
    }
}