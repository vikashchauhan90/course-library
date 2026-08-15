using CourseLibrary.Infrastructure.Idempotency;
using CourseLibrary.Application.Abstractions.Idempotency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace CourseLibrary.Api.Configuration.Idempotency.Filters;

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
            .GetRequiredService<IIdempotencyStore>();

        var cacheKey = $"idempotency:{key}";
        var storedResponse = await store.GetOrCreateAsync(
            cacheKey,
            async cancellationToken =>
            {
                var executedContext = await next();
                return IdempotencyEntry.Empty;
            },
            _ttl,
            context.HttpContext.RequestAborted);

        if (!storedResponse.IsEmpty)
        {
            var result = new ContentResult
            {
                StatusCode = storedResponse.ResponseStatusCode,
                ContentType = storedResponse.ResponseContentType,
                Content = Encoding.UTF8.GetString(storedResponse.ResponseBody)
            };

            context.Result = result;
            return;

        }

        var actionResultContext = await next();
        var resultEntry = await CreateEntryAsync(context, actionResultContext);
        if (resultEntry is not null)
        {
            await store.StoreAsync(cacheKey, resultEntry, _ttl, context.HttpContext.RequestAborted);
        }
    }

    private static async Task<IdempotencyEntry?> CreateEntryAsync(
        ActionExecutingContext context,
        ActionExecutedContext actionResultContext)
    {

        var request = context.HttpContext.Request;
        var requestPath = request.Path.ToString();
        var requestMethod = request.Method;
        var requestContentType = request.ContentType;

        if (actionResultContext.Result is ObjectResult objectResult)
        {
            return new IdempotencyEntry(
                requestPath,
                requestMethod,
                requestContentType,
                objectResult.StatusCode ?? 200,
                objectResult.ContentTypes.FirstOrDefault() ?? MediaTypeNames.Application.Json,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(objectResult.Value)));
        }

        if (actionResultContext.Result is JsonResult jsonResult)
        {
            return new IdempotencyEntry(
                requestPath,
                requestMethod,
                requestContentType,
                200,
                MediaTypeNames.Application.Json,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(jsonResult.Value)));
        }

        if (actionResultContext.Result is ContentResult contentResult)
        {
            return new IdempotencyEntry(
                requestPath,
                requestMethod,
                requestContentType,
                contentResult.StatusCode ?? 200,
                contentResult.ContentType ?? MediaTypeNames.Text.Plain,
                Encoding.UTF8.GetBytes(contentResult.Content ?? string.Empty));
        }

        if (actionResultContext.Result is StatusCodeResult statusCodeResult)
        {
            return new IdempotencyEntry(
                requestPath,
                requestMethod,
                requestContentType,
                statusCodeResult.StatusCode,
                MediaTypeNames.Text.Plain,
                Encoding.UTF8.GetBytes(string.Empty));
        }

        return null;
    }
}
