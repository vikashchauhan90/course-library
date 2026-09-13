namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct Code(string Value)
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

    public static explicit operator Code(string value)
    {
        return Create(value);
    }

    public static explicit operator string(Code code)
    {
        return code.Value;
    }
    public override string ToString()
    {
        return Value;
    }
}