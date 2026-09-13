namespace CourseLibrary.Domain.ValueObjects;

public readonly struct AuditAction
{
    public AuditAction(Guid value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException(
                $"Unknown audit action type '{value}'.",
                nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }
    public static AuditAction Add { get; } = new(AddValue);
    public static AuditAction Updated { get; } = new(UpdatedValue);
    public static AuditAction Deleted { get; } = new(DeletedValue);

    public override string ToString() => Value.ToString();
    public static bool TryParse(
        string? value,
        out AuditAction status)
    {
        if (Guid.TryParse(value, out var parsed) &&
            IsValid(parsed))
        {
            status = new AuditAction(parsed);
            return true;
        }

        status = default;
        return false;
    }

    public static explicit operator AuditAction(Guid value)
    {
        return new AuditAction(value);
    }

    public static explicit operator Guid(AuditAction action)
    {
        return action.Value;
    }


    public static explicit operator AuditAction(string value)
    {
        _ = TryParse(value, out var parsed);
        return parsed;
    }

    public static explicit operator string(AuditAction action) => action.ToString();

    private static readonly Guid AddValue =
    new("30000000-0000-0000-0000-000000000001");

    private static readonly Guid UpdatedValue =
        new("30000000-0000-0000-0000-000000000002");

    private static readonly Guid DeletedValue =
        new("30000000-0000-0000-0000-000000000003");

    private static readonly HashSet<Guid> KnownActions =
    [
        AddValue,
        UpdatedValue,
        DeletedValue
    ];

    public static bool IsValid(Guid value) => KnownActions.Contains(value);
}