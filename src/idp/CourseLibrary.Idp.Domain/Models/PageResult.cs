namespace CourseLibrary.Idp.Domain.Models;

public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    string? ContinuationToken,
    bool HasMore)
{

    public static PageResult<T> Empty => new(Array.Empty<T>(), null, false);

}