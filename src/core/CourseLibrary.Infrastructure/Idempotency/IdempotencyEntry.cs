namespace CourseLibrary.Infrastructure.Idempotency;

public sealed class IdempotencyEntry
{
    public string RequestPath { get; init; } = string.Empty;
    public string RequestMethod { get; init; } = string.Empty;
    public string? RequestContentType { get; init; }
    public int ResponseStatusCode { get; init; }
    public string ResponseContentType { get; init; } = "application/json";
    public byte[] ResponseBody { get; init; } = Array.Empty<byte>();
}
