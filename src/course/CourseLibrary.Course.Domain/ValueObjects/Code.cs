using CourseLibrary.Core.Domain.Abstractions.ValueObjects;

namespace CourseLibrary.Course.Domain.ValueObjects;

public readonly record struct Code(string Value)
    : IStronglyTypedValue<string>
{
    public static Code Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Code cannot be empty.",
                nameof(value));
        }

        return new Code(value.Trim());
    }

    public static implicit operator Code(string value)
    {
        return Create(value);
    }

    public static implicit operator string(Code code)
    {
        return code.Value;
    }
    public override string ToString()
    {
        return Value;
    }
}