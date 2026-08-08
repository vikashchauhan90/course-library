using CourseLibrary.Infrastructure.Idempotency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mime;
using System.Text;
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
            var stored = await store.GetAsync(cacheKey, context.HttpContext.RequestAborted);
            if (stored is not null)
            {
                var result = new ContentResult
                {
                    StatusCode = stored.ResponseStatusCode,
                    ContentType = stored.ResponseContentType,
                    Content = Encoding.UTF8.GetString(stored.ResponseBody)
                };

                context.Result = result;
                return;
            }
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
        var entry = new IdempotencyEntry
        {
            RequestPath = request.Path,
            RequestMethod = request.Method,
            RequestContentType = request.ContentType
        };

        if (actionResultContext.Result is ObjectResult objectResult)
        {
            return new IdempotencyEntry
            {
                RequestPath = entry.RequestPath,
                RequestMethod = entry.RequestMethod,
                RequestContentType = entry.RequestContentType,
                ResponseStatusCode = objectResult.StatusCode ?? 200,
                ResponseContentType = objectResult.ContentTypes.FirstOrDefault() ?? MediaTypeNames.Application.Json,
                ResponseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(objectResult.Value))
            };
        }

        if (actionResultContext.Result is JsonResult jsonResult)
        {
            return new IdempotencyEntry
            {
                RequestPath = entry.RequestPath,
                RequestMethod = entry.RequestMethod,
                RequestContentType = entry.RequestContentType,
                ResponseStatusCode = 200,
                ResponseContentType = MediaTypeNames.Application.Json,
                ResponseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(jsonResult.Value))
            };
        }

        if (actionResultContext.Result is ContentResult contentResult)
        {
            return new IdempotencyEntry
            {
                RequestPath = entry.RequestPath,
                RequestMethod = entry.RequestMethod,
                RequestContentType = entry.RequestContentType,
                ResponseStatusCode = contentResult.StatusCode ?? 200,
                ResponseContentType = contentResult.ContentType ?? MediaTypeNames.Text.Plain,
                ResponseBody = Encoding.UTF8.GetBytes(contentResult.Content ?? string.Empty)
            };
        }

        if (actionResultContext.Result is StatusCodeResult statusCodeResult)
        {
            return new IdempotencyEntry
            {
                RequestPath = entry.RequestPath,
                RequestMethod = entry.RequestMethod,
                RequestContentType = entry.RequestContentType,
                ResponseStatusCode = statusCodeResult.StatusCode,
                ResponseContentType = MediaTypeNames.Text.Plain,
                ResponseBody = Encoding.UTF8.GetBytes(string.Empty)
            };
        }

        return null;
    }
}
