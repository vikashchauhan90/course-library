namespace CourseLibrary.Domain.Events;

public static class CourseEventType
{
    public static readonly Guid Created =
        new("10000000-0000-0000-0000-000000000001");

    public static readonly Guid Updated =
        new("10000000-0000-0000-0000-000000000002");

    public static readonly Guid Deleted =
        new("10000000-0000-0000-0000-000000000003");

    public static readonly Guid Retired =
        new("10000000-0000-0000-0000-000000000004");

    private static readonly HashSet<Guid> KnownTypes =
    [
        Created,
        Updated,
        Deleted,
        Retired
    ];

    public static bool IsValid(Guid eventType)
    {
        return KnownTypes.Contains(eventType);
    }

    public static bool TryParse(
        string? value,
        out Guid eventType)
    {
        if (Guid.TryParse(value, out eventType) &&
            IsValid(eventType))
        {
            return true;
        }

        eventType = Guid.Empty;
        return false;
    }
}