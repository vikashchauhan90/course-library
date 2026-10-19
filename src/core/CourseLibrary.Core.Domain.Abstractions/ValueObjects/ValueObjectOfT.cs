namespace CourseLibrary.Core.Domain.Abstractions.ValueObjects;

public abstract class ValueObject<TSelf>
    : IEquatable<TSelf>
    where TSelf : ValueObject<TSelf>, IValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(TSelf? other)
    {
        return other is not null &&
               GetEqualityComponents()
                   .SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        return obj is TSelf other &&
               Equals(other);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (object? component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }
}