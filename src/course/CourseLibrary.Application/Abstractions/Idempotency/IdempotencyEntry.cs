namespace CourseLibrary.Application.Abstractions.Idempotency;

public sealed class IdempotencyEntry
{
    public required string Key { get; init; }
    public IdempotencyStatus Status { get; private set; }
    public int? ResponseStatusCode { get; private set; }
    public string? ResponseContentType { get; private set; }
    public byte[]? ResponseBody { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; private set; }
    private IdempotencyEntry()
    {
    }

    public static IdempotencyEntry CreateProcessing(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return new IdempotencyEntry
        {
            Key = key,
            Status = IdempotencyStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static IdempotencyEntry CreateCompleted(
       string key,
       int responseStatusCode,
       string? responseContentType,
       byte[] responseBody)
    {
        var entry = CreateProcessing(key);

        entry.Complete(
            responseStatusCode,
            responseContentType,
            responseBody);

        return entry;
    }

    public void Complete(
        int responseStatusCode,
        string? responseContentType,
        byte[] responseBody)
    {
        ArgumentNullException.ThrowIfNull(responseBody);

        Status = IdempotencyStatus.Completed;
        ResponseStatusCode = responseStatusCode;
        ResponseContentType = responseContentType;
        ResponseBody = responseBody;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}