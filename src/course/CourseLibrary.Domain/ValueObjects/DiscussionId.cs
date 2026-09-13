namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct DiscussionId(Guid Value)
{
    public static DiscussionId New()
    {
        return new DiscussionId(Guid.NewGuid());
    }

    public static DiscussionId Empty =>
        new(Guid.Empty);

    public static explicit operator DiscussionId(Guid value)
    {
        return new DiscussionId(value);
    }

    public static explicit operator Guid(DiscussionId id)
    {
        return id.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}