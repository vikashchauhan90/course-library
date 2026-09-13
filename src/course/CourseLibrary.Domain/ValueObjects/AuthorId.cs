namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct AuthorId(string Value)
{
    public static AuthorId New()
    {
        return new AuthorId(Guid.NewGuid().ToString());
    }

    public static AuthorId Empty =>
      new(string.Empty);

    public static explicit operator AuthorId(string value)
    {
        return new AuthorId(value);
    }

    public static explicit operator string(AuthorId id)
    {
        return id.Value;
    }
    public override string ToString()
    {
        return Value;
    }
}
