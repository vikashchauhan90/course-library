using CourseLibrary.Core.Domain.Abstractions.ValueObjects;

namespace CourseLibrary.Course.Domain.ValueObjects;

public readonly record struct CourseId(string Value)
    : IStronglyTypedValue<string>
{
    public static CourseId New()
    {
        return new CourseId(Guid.NewGuid().ToString("N"));
    }


    public static implicit operator CourseId(string value)
    {
        return new CourseId(value);
    }

    public static implicit operator string(CourseId id)
    {
        return id.Value;
    }
    public override string ToString()
    {
        return Value.ToString();
    }
}
