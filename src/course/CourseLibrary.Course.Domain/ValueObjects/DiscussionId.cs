using CourseLibrary.Core.Domain.Abstractions.ValueObjects;

namespace CourseLibrary.Course.Domain.ValueObjects;

public readonly record struct DiscussionId(string Value)
    : IStronglyTypedValue<string>
{
    public static DiscussionId New()
    {
        return new DiscussionId(Guid.NewGuid().ToString("N"));
    }

    public static implicit operator DiscussionId(string value)
    {
        return new DiscussionId(value);
    }

    public static implicit operator string(DiscussionId id)
    {
        return id.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}