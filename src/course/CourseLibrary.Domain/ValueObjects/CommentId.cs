namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct CommentId(Guid Value)
{
    public static CommentId New()
    {
        return new CommentId(Guid.NewGuid());
    }

    public static CommentId Empty =>
        new(Guid.Empty);


    public static explicit operator CommentId(Guid value)
    {
        return new CommentId(value);
    }

    public static explicit operator Guid(CommentId id)
    {
        return id.Value;
    }
    public override string ToString()
    {
        return Value.ToString();
    }
}