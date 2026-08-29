using CourseLibrary.Infrastructure.OutputCache;
using Microsoft.AspNetCore.OutputCaching;

public sealed class IdempotencyOutputCachePolicy(
    ILogger<IdempotencyOutputCachePolicy> logger)
    : IOutputCachePolicy
{
    private const string IdempotencyHeader = "Idempotency-Key";

    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(
                IdempotencyHeader,
                out var idempotencyKey) ||
            string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.EnableOutputCaching = false;

            logger.LogDebug(
                "Output cache disabled because {IdempotencyHeader} is missing for {RequestMethod} {RequestPath}",
                IdempotencyHeader,
                request.Method,
                request.Path);

            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = true;
        context.AllowCacheStorage = true;
        context.AllowLocking = true;
        context.CacheVaryByRules.QueryKeys = "*";
        context.CacheVaryByRules.HeaderNames = new[]
        {
            IdempotencyHeader
        };

        context.Tags.Add("idempotency");
        context.Tags.Add($"idempotency:{idempotencyKey}");

        logger.LogDebug(
            "Output cache enabled for {RequestMethod} {RequestPath} with idempotency key {IdempotencyKey}",
            request.Method,
            request.Path,
            idempotencyKey);

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;

        var ttl = context.ResponseExpirationTimeSpan?.TotalSeconds ?? 0;

        var outputCache = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IOutputCacheDiagnostics>();

        outputCache.Hit = true;
        outputCache.ExpirationDuration = context.ResponseExpirationTimeSpan;

        logger.LogDebug(
            "Output cache HIT for {RequestMethod} {RequestPath} with TTL {CacheTtlSeconds}s",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            ttl);

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;

        var ttl = context.ResponseExpirationTimeSpan?.TotalSeconds ?? 0;

        var outputCache = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IOutputCacheDiagnostics>();

        outputCache.Hit = false;
        outputCache.ExpirationDuration = context.ResponseExpirationTimeSpan;

        if (response.Headers.SetCookie.Count > 0)
        {
            context.AllowCacheStorage = false;

            logger.LogDebug(
                "Output cache storage disabled for {RequestMethod} {RequestPath} because response contains Set-Cookie",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);

            return ValueTask.CompletedTask;
        }

        if (response.StatusCode is not (
            StatusCodes.Status200OK or
            StatusCodes.Status201Created or
            StatusCodes.Status204NoContent))
        {
            context.AllowCacheStorage = false;

            logger.LogDebug(
                "Output cache storage disabled for {RequestMethod} {RequestPath} because response status code is {StatusCode}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                response.StatusCode);

            return ValueTask.CompletedTask;
        }

        logger.LogDebug(
            "Output cache MISS for {RequestMethod} {RequestPath}. Response will be stored with TTL {CacheTtlSeconds}s",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            ttl);

        return ValueTask.CompletedTask;
    }
}