using Microsoft.AspNetCore.OutputCaching;

namespace CourseLibrary.Api.Configuration.OutputCache.Policies;

public sealed class IdempotencyOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(
                "Idempotency-Key",
                out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.EnableOutputCaching = false;
            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;

        context.CacheVaryByRules.HeaderNames = new[]
        {
            "Idempotency-Key"
        };

        context.Tags.Add("idempotency");
        context.Tags.Add($"idempotency:{idempotencyKey}");

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}