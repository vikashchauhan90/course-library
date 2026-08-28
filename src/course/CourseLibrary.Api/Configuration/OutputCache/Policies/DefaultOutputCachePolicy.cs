using CourseLibrary.Infrastructure.OutputCache;
using Microsoft.AspNetCore.OutputCaching;

namespace CourseLibrary.Api.Configuration.OutputCache.Policies;

public sealed class DefaultOutputCachePolicy(
    ILogger<DefaultOutputCachePolicy> logger)
    : IOutputCachePolicy
{
    private static readonly HashSet<string> CacheableMethods = new()
    {
        HttpMethods.Get,
        HttpMethods.Head
    };

    public ValueTask CacheRequestAsync(
        OutputCacheContext context,
        CancellationToken cancellationToken)
    {
        var request = context.HttpContext.Request;
        var outputCache = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IOutputCacheDiagnostics>();

        // Default output cache is only for safe HTTP methods.
        if (!CacheableMethods.Contains(request.Method))
        {
            context.EnableOutputCaching = false;

            logger.LogDebug(
                "Output cache disabled for {RequestMethod} {RequestPath}: HTTP method is not cacheable",
                request.Method,
                request.Path);

            return ValueTask.CompletedTask;
        }

        context.EnableOutputCaching = true;

        logger.LogDebug(
            "Output cache enabled for {RequestMethod} {RequestPath}",
            request.Method,
            request.Path);

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
        logger.LogDebug(
                    "Output cache MISS for {RequestMethod} {RequestPath}. Response will be stored with TTL {CacheTtlSeconds}s",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    ttl);

        // Never cache responses that set cookies.
        if (response.Headers.SetCookie.Count > 0)
        {
            context.AllowCacheStorage = false;

            logger.LogDebug(
                "Output cache storage disabled for {RequestMethod} {RequestPath}: response contains Set-Cookie",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);

            return ValueTask.CompletedTask;
        }

        // Cache successful responses only.
        if (response.StatusCode is not (
            StatusCodes.Status200OK or
            StatusCodes.Status201Created or
            StatusCodes.Status204NoContent))
        {
            context.AllowCacheStorage = false;

            logger.LogDebug(
                "Output cache storage disabled for {RequestMethod} {RequestPath}: status code {StatusCode} is not cacheable",
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