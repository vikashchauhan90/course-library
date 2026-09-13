namespace CourseLibrary.Domain.Events;

public static class DqlMessageStatus
{
    public static readonly Guid DeadLettered =
        new("20000000-0000-0000-0000-000000000001");

    public static readonly Guid ReplayPending =
        new("20000000-0000-0000-0000-000000000002");

    public static readonly Guid Replayed =
        new("20000000-0000-0000-0000-000000000003");

    public static readonly Guid ReplayFailed =
        new("20000000-0000-0000-0000-000000000004");

    public static readonly Guid PermanentlyFailed =
        new("20000000-0000-0000-0000-000000000005");

    private static readonly HashSet<Guid> KnownStatuses =
    [
        DeadLettered,
        ReplayPending,
        Replayed,
        ReplayFailed,
        PermanentlyFailed
    ];

    public static bool IsValid(Guid status)
    {
        return KnownStatuses.Contains(status);
    }

    public static bool TryParse(
        string? value,
        out Guid status)
    {
        if (Guid.TryParse(value, out status) &&
            IsValid(status))
        {
            return true;
        }

        status = Guid.Empty;
        return false;
    }
}
