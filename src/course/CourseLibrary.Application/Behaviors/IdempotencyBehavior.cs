using CourseLibrary.Application.Abstractions.Idempotency;
using CourseLibrary.Application.Abstractions.RequestContext;
using MediatorForge.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CourseLibrary.Application.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>(
    IRequestContext requestContext,
    IIdempotencyStore idempotencyStore,
    ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : notnull
{
    private static readonly TimeSpan Ttl =
        TimeSpan.FromMinutes(5);

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var idempotencyKey = requestContext.IdempotencyKey;

        // No idempotency key means normal command execution.
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return await next();
        }

        var existing = await idempotencyStore.GetAsync(
            idempotencyKey,
            ct);

        if (existing is not null)
        {
            if (existing.Status == IdempotencyStatus.Completed)
            {
                logger.LogDebug(
                    "Returning cached idempotency response for key {IdempotencyKey} and request {RequestName}.",
                    idempotencyKey,
                    typeof(TRequest).Name);

                return DeserializeResponse(existing);
            }

            logger.LogDebug(
                "Idempotency request already processing for key {IdempotencyKey} and request {RequestName}.",
                idempotencyKey,
                typeof(TRequest).Name);

            throw new IdempotencyRequestInProgressException(
                idempotencyKey);
        }

        var processingEntry =
            IdempotencyEntry.CreateProcessing(idempotencyKey);

        var acquired = await idempotencyStore.TryAcquireAsync(
            processingEntry,
            Ttl,
            ct);

        if (!acquired)
        {
            var current = await idempotencyStore.GetAsync(
                idempotencyKey,
                ct);

            if (current?.Status == IdempotencyStatus.Completed)
            {
                logger.LogDebug(
                    "Returning concurrently completed idempotency response for key {IdempotencyKey} and request {RequestName}.",
                    idempotencyKey,
                    typeof(TRequest).Name);

                return DeserializeResponse(current);
            }

            throw new IdempotencyRequestInProgressException(
                idempotencyKey);
        }

        logger.LogDebug(
            "Acquired idempotency key {IdempotencyKey} for request {RequestName}.",
            idempotencyKey,
            typeof(TRequest).Name);

        var response = await next();

        var responseBody = JsonSerializer.SerializeToUtf8Bytes(
            response);

        processingEntry.Complete(
            StatusCodes.Status200OK,
            "application/json",
            responseBody);

        await idempotencyStore.StoreAsync(
            idempotencyKey,
            processingEntry,
            Ttl,
            ct);

        return response;
    }

    private static TResponse DeserializeResponse(
        IdempotencyEntry entry)
    {
        if (entry.ResponseBody is null)
        {
            throw new InvalidOperationException(
                $"Completed idempotency entry '{entry.Key}' does not contain a response.");
        }

        return JsonSerializer.Deserialize<TResponse>(
            entry.ResponseBody)
            ?? throw new InvalidOperationException(
                $"Unable to deserialize idempotency response for key '{entry.Key}'.");
    }
}