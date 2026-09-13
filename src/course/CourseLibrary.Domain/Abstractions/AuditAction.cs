namespace CourseLibrary.Domain.Abstractions;

public static class AuditAction
{
    public static readonly Guid Add =
        new("30000000-0000-0000-0000-000000000001");

    public static readonly Guid Updated =
        new("30000000-0000-0000-0000-000000000002");

    public static readonly Guid Deleted =
        new("30000000-0000-0000-0000-000000000003");

    private static readonly HashSet<Guid> KnownActions =
    [
        Add,
        Updated,
        Deleted
    ];

    public static bool IsValid(Guid value)
    {
        return KnownActions.Contains(value);
    }

    public static bool TryParse(
        string? value,
        out Guid action)
    {
        if (Guid.TryParse(value, out action) &&
            IsValid(action))
        {
            return true;
        }

        action = Guid.Empty;
        return false;
    }
}