namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct CourseId(Guid Value)
{
    public static CourseId New()
    {
        return new CourseId(Guid.NewGuid());
    }

    public static CourseId Empty =>
        new(Guid.Empty);


    public static explicit operator CourseId(Guid value)
    {
        return new CourseId(value);
    }

    public static explicit operator Guid(CourseId id)
    {
        return id.Value;
    }

    public static explicit operator CourseId(string value)
    {
        if (Guid.TryParse(value, out var parsed))
        {
            return new CourseId(parsed);
        }
        throw new ArgumentException(
            $"Invalid course ID '{value}'.",
            nameof(value));
    }

    public static explicit operator string(CourseId id) => id.Value.ToString();
    public override string ToString()
    {
        return Value.ToString();
    }
}
