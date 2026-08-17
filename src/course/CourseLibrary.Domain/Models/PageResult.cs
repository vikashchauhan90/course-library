namespace CourseLibrary.Domain.Models;

public sealed record PageResult<T>(
    IReadOnlyList<T> Items,
    string? ContinuationToken,
    bool HasMore)
{
    /// <summary>
    /// Creates a paged result with a projection (map items to a different type).
    /// </summary>
    /// <typeparam name="TResult">Target type after projection.</typeparam>
    /// <param name="selector">Projection function.</param>
    /// <returns>A new PagedResult with projected items.</returns>
    public PageResult<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        if (selector is null)
            throw new ArgumentNullException(nameof(selector));

        return new PageResult<TResult>(
            Items.Select(selector).ToList(),
            ContinuationToken,
            HasMore);
    }
}