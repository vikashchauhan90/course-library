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
    public override string ToString()
    {
        return Value.ToString();
    }
}
