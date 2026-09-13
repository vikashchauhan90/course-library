namespace CourseLibrary.Application.Abstractions.Idempotency;

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
        (ResponseBody is null || ResponseBody.Length == 0);

    public static IdempotencyEntry GetIdempotencyEntry(byte[] ResponseBody) =>
        new(
            string.Empty,
            string.Empty,
            null,
            0,
            string.Empty,
            ResponseBody
            );

}