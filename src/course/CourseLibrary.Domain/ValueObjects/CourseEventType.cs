namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct CourseEventType
{
    // Static fields are initialized in declaration order.
    // Initialize all fields required by validation before defining static
    // instances that depend on them, to avoid accessing uninitialized state
    // during type initialization.
    private static readonly Guid CreatedValue = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid UpdatedValue = new("10000000-0000-0000-0000-000000000002");
    private static readonly Guid DeletedValue = new("10000000-0000-0000-0000-000000000003");
    private static readonly Guid RetiredValue = new("10000000-0000-0000-0000-000000000004");

    private static readonly HashSet<Guid> KnownEventTypes = new()
    {
        CreatedValue,
        UpdatedValue,
        DeletedValue,
        RetiredValue
    };

    public CourseEventType(Guid value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException(
                $"Unknown course event type '{value}'.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static CourseEventType Created { get; } = new(
        CreatedValue);

    public static CourseEventType Updated { get; } = new(
        UpdatedValue);

    public static CourseEventType Deleted { get; } = new(
        DeletedValue);

    public static CourseEventType Retired { get; } = new(
        RetiredValue);

    public static bool TryParse(
        string? value,
        out CourseEventType eventType)
    {
        if (Guid.TryParse(value, out var parsed) &&
            IsValid(parsed))
        {
            eventType = new CourseEventType(parsed);
            return true;
        }

        eventType = default;
        return false;
    }

    public static explicit operator CourseEventType(Guid value) => new(value);
    public static explicit operator Guid(CourseEventType eventType) => eventType.Value;
    public static explicit operator CourseEventType(string value)
    {
        _ = TryParse(value, out var parsed);
        return parsed;
    }
    public static explicit operator string(CourseEventType eventType) => eventType.ToString();
    public override string ToString() => Value.ToString();

    private static bool IsValid(Guid value) => KnownEventTypes.Contains(value);

}