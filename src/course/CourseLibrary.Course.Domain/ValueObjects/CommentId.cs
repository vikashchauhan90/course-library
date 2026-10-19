using CourseLibrary.Core.Domain.Abstractions.ValueObjects;

namespace CourseLibrary.Course.Domain.ValueObjects;

public readonly record struct CommentId(string Value)
    : IStronglyTypedValue<string>
{
    public static CommentId New()
    {
        return new CommentId(Guid.NewGuid().ToString("N"));
    }


    public static implicit operator CommentId(string value)
    {
        return new CommentId(value);
    }

    public static implicit operator string(CommentId id)
    {
        return id.Value;
    }
    public override string ToString()
    {
        return Value.ToString();
    }
}