using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace CourseLibrary.Api.Configuration.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class IdempotencyAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _headerName;
    private readonly TimeSpan _ttl;

    public IdempotencyAttribute(string headerName = "Idempotency-Key", int ttlSeconds = 300)
    {
        _headerName = headerName;
        _ttl = TimeSpan.FromSeconds(ttlSeconds);
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var key) || string.IsNullOrWhiteSpace(key))
        {
            context.Result = new BadRequestObjectResult($"Missing or empty {_headerName} header.");
            return;
        }

        var store = context.HttpContext.RequestServices
            .GetRequiredService<CourseLibrary.Infrastructure.Idempotency.IIdempotencyStore>();

        var cacheKey = $"idempotency:{key}";
        if (await store.ExistsAsync(cacheKey, context.HttpContext.RequestAborted))
        {
            var stored = await store.GetResponseAsync(cacheKey, context.HttpContext.RequestAborted);
            if (stored is not null)
            {
                context.Result = new JsonResult(stored);
                return;
            }
        }

        var actionResultContext = await next();

        if (actionResultContext.Result is ObjectResult objectResult)
        {
            await store.StoreResponseAsync(cacheKey, objectResult.Value ?? new { }, _ttl, context.HttpContext.RequestAborted);
        }
        else if (actionResultContext.Result is JsonResult jsonResult)
        {
            await store.StoreResponseAsync(cacheKey, jsonResult.Value ?? new { }, _ttl, context.HttpContext.RequestAborted);
        }
        else if (actionResultContext.Result is ContentResult contentResult)
        {
            await store.StoreResponseAsync(cacheKey, contentResult.Content ?? string.Empty, _ttl, context.HttpContext.RequestAborted);
        }
        else
        {
            await store.StoreResponseAsync(cacheKey, new { status = actionResultContext.Result?.ToString() ?? string.Empty }, _ttl, context.HttpContext.RequestAborted);
        }
    }
}
