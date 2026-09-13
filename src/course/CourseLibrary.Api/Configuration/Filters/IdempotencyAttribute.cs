using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Infrastructure.Configuration.Idempotency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace CourseLibrary.Api.Configuration.Idempotency.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class IdempotencyAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _headerName = IdempotencyHeader.HeaderName;
    private readonly TimeSpan _ttl;

    public IdempotencyAttribute(int ttlSeconds = 300)
    {
        if (ttlSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ttlSeconds),
                ttlSeconds,
                "Idempotency TTL must be greater than zero.");
        }

        _ttl = TimeSpan.FromSeconds(ttlSeconds);
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue(
                _headerName,
                out var headerValue) ||
            string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            context.Result = new BadRequestObjectResult(
                $"Missing or empty {_headerName} header.");

            return;
        }

        var idempotencyKey = headerValue.ToString();

        var store = context.HttpContext.RequestServices
            .GetRequiredService<IIdempotencyStore>();

        var cancellationToken = context.HttpContext.RequestAborted;

        // The idempotency key itself is the identity.
        var storedEntry = await store.GetAsync(
            idempotencyKey,
            cancellationToken);

        if (storedEntry is not null)
        {
            if (storedEntry.Status == IdempotencyStatus.Completed)
            {
                context.Result = CreateReplayResult(storedEntry);
                return;
            }

            // Another request currently owns this idempotency key.
            context.Result = new ConflictObjectResult(
                new
                {
                    error = "idempotency_request_in_progress",
                    message = "A request with this idempotency key is already being processed."
                });

            return;
        }

        // Atomically claim the idempotency key before executing the action.
        var processingEntry = IdempotencyEntry.CreateProcessing(
            idempotencyKey);

        var acquired = await store.TryAcquireAsync(
            processingEntry,
            _ttl,
            cancellationToken);

        if (!acquired)
        {
            // Another request won the race between GetAsync and TryAcquireAsync.
            var existingEntry = await store.GetAsync(
                idempotencyKey,
                cancellationToken);

            if (existingEntry?.Status == IdempotencyStatus.Completed)
            {
                context.Result = CreateReplayResult(existingEntry);
                return;
            }

            context.Result = new ConflictObjectResult(
                new
                {
                    error = "idempotency_request_in_progress",
                    message = "A request with this idempotency key is already being processed."
                });

            return;
        }

        var actionResultContext = await next();

        // Do not overwrite the processing entry when the action failed
        // without producing a valid response.
        if (actionResultContext.Exception is not null &&
            !actionResultContext.ExceptionHandled)
        {
            return;
        }

        var resultEntry = CreateEntry(
            idempotencyKey,
            actionResultContext);

        if (resultEntry is null)
        {
            return;
        }

        await store.StoreAsync(
            idempotencyKey,
            resultEntry,
            _ttl,
            cancellationToken);
    }

    private static IActionResult CreateReplayResult(
        IdempotencyEntry entry)
    {
        return new ContentResult
        {
            StatusCode = entry.ResponseStatusCode ?? StatusCodes.Status200OK,
            ContentType = entry.ResponseContentType
                ?? MediaTypeNames.Application.Json,
            Content = entry.ResponseBody is { Length: > 0 }
                ? Encoding.UTF8.GetString(entry.ResponseBody)
                : string.Empty
        };
    }

    private static IdempotencyEntry? CreateEntry(
        string idempotencyKey,
        ActionExecutedContext actionResultContext)
    {
        if (actionResultContext.Result is ObjectResult objectResult)
        {
            return IdempotencyEntry.CreateCompleted(
                idempotencyKey,
                objectResult.StatusCode
                    ?? StatusCodes.Status200OK,
                objectResult.ContentTypes.FirstOrDefault()
                    ?? MediaTypeNames.Application.Json,
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(objectResult.Value)));
        }

        if (actionResultContext.Result is JsonResult jsonResult)
        {
            return IdempotencyEntry.CreateCompleted(
                idempotencyKey,
                jsonResult.StatusCode
                    ?? StatusCodes.Status200OK,
                jsonResult.ContentType
                    ?? MediaTypeNames.Application.Json,
                Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(jsonResult.Value)));
        }

        if (actionResultContext.Result is ContentResult contentResult)
        {
            return IdempotencyEntry.CreateCompleted(
                idempotencyKey,
                contentResult.StatusCode
                    ?? StatusCodes.Status200OK,
                contentResult.ContentType
                    ?? MediaTypeNames.Text.Plain,
                Encoding.UTF8.GetBytes(
                    contentResult.Content ?? string.Empty));
        }

        if (actionResultContext.Result is StatusCodeResult statusCodeResult)
        {
            return IdempotencyEntry.CreateCompleted(
                idempotencyKey,
                statusCodeResult.StatusCode,
                MediaTypeNames.Text.Plain,
                Array.Empty<byte>());
        }

        return null;
    }
}
