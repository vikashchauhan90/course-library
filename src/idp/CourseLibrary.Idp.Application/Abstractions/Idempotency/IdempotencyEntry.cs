namespace CourseLibrary.Idp.Application.Abstractions.Idempotency;

public sealed record IdempotencyEntry(
    string RequestPath,
    string RequestMethod,
    string? RequestContentType,
    int ResponseStatusCode,
    string ResponseContentType,
    byte[] ResponseBody)
{
    public static IdempotencyEntry Empty =>
        new(
            RequestPath: string.Empty,
            RequestMethod: string.Empty,
            RequestContentType: null,
            ResponseStatusCode: 0,
            ResponseContentType: string.Empty,
            ResponseBody: Array.Empty<byte>());

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(RequestPath) &&
        string.IsNullOrWhiteSpace(RequestMethod) &&
        string.IsNullOrWhiteSpace(ResponseContentType) &&
        ResponseStatusCode == 0 &&
        (ResponseBody is null || ResponseBody.Length == 0);
}