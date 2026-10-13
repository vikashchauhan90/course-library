using CourseLibrary.Core.Domain.Abstractions.ValueObjects;

namespace CourseLibrary.Course.Domain.ValueObjects;

public readonly record struct AuthorId(string Value)
    : IStronglyTypedValue<string>
{
    public static AuthorId New()
        => new(Guid.NewGuid().ToString("N"));

    public static implicit operator AuthorId(string value)
        => new(value);

    public static implicit operator string(AuthorId id)
        => id.Value;

    public override string ToString()
        => Value;
}