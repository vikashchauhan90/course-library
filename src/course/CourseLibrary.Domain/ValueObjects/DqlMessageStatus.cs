namespace CourseLibrary.Domain.ValueObjects;

public readonly struct DqlMessageStatus
{

    public DqlMessageStatus(Guid value)
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

    public static DqlMessageStatus DeadLettered { get; } = new(
        DeadLetteredValue);

    public static DqlMessageStatus ReplayPending { get; } = new(
        ReplayPendingValue);

    public static DqlMessageStatus Replayed { get; } = new(
        ReplayedValue);

    public static DqlMessageStatus ReplayFailed { get; } = new(
        ReplayFailedValue);

    public static DqlMessageStatus PermanentlyFailed { get; } = new(
        PermanentlyFailedValue);
    public static bool TryParse(
        string? value,
        out DqlMessageStatus status)
    {
        if (Guid.TryParse(value, out var parsed) &&
            IsValid(parsed))
        {
            status = new DqlMessageStatus(parsed);
            return true;
        }

        status = default;
        return false;
    }

    public static explicit operator DqlMessageStatus(Guid value) => new(value);
    public static explicit operator Guid(DqlMessageStatus status) => status.Value;
    public static explicit operator DqlMessageStatus(string value)
    {
        _ = TryParse(value, out var parsed);
        return parsed;
    }
    public static explicit operator string(DqlMessageStatus status) => status.ToString();
    public override string ToString() => Value.ToString();
    private static readonly Guid DeadLetteredValue =
        new("20000000-0000-0000-0000-000000000001");

    private static readonly Guid ReplayPendingValue =
        new("20000000-0000-0000-0000-000000000002");

    private static readonly Guid ReplayedValue =
        new("20000000-0000-0000-0000-000000000003");

    private static readonly Guid ReplayFailedValue =
        new("20000000-0000-0000-0000-000000000004");

    private static readonly Guid PermanentlyFailedValue =
        new("20000000-0000-0000-0000-000000000005");

    private static readonly HashSet<Guid> KnownStatuses =
    [
        DeadLetteredValue,
        ReplayPendingValue,
        ReplayedValue,
        ReplayFailedValue,
        PermanentlyFailedValue
    ];
    public static bool IsValid(Guid status) => KnownStatuses.Contains(status);
}
