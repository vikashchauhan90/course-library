namespace CourseLibrary.Application.Abstractions.Idempotency;

public sealed record IdempotencyEntry(
    string RequestPath,
    string RequestMethod,
    string? RequestContentType,
    int ResponseStatusCode,
    string ResponseContentType,
    byte[] ResponseBody);