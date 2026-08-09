using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.Gateway.Configuration.Exceptions;

public sealed class GatewayExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GatewayExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var mapping = MapException(exception, context);
        var traceId = GetTraceId(context);

        LogException(
            context,
            exception,
            mapping.StatusCode,
            traceId);

        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "Cannot write gateway error response because the response has already started. " +
                "TraceId: {TraceId}",
                traceId);

            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = mapping.StatusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = mapping.Type,
            Title = mapping.Title,
            Status = mapping.StatusCode,
            Detail = mapping.Detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            context.RequestAborted);
    }

    private static GatewayExceptionMapping MapException(
        Exception exception,
         HttpContext context)
    {
        return exception switch
        {
            // Downstream service could not be reached.
            HttpRequestException =>
                new GatewayExceptionMapping(
                    StatusCodes.Status502BadGateway,
                    "https://api.courselibrary.com/errors/bad-gateway",
                    "Bad gateway",
                    "The gateway could not communicate with the downstream service."),

            // Request exceeded configured timeout.
            TimeoutException =>
                new GatewayExceptionMapping(
                    StatusCodes.Status504GatewayTimeout,
                    "https://api.courselibrary.com/errors/gateway-timeout",
                    "Gateway timeout",
                    "The downstream service did not respond within the allowed time."),

            // Client disconnected.
            OperationCanceledException
                when context.RequestAborted.IsCancellationRequested =>
                new GatewayExceptionMapping(
                    StatusCodes.Status408RequestTimeout,
                    "https://api.courselibrary.com/errors/request-timeout",
                    "Request timeout",
                    "The request was cancelled or timed out."),

            // Generic cancellation/timeout.
            OperationCanceledException =>
                new GatewayExceptionMapping(
                    StatusCodes.Status504GatewayTimeout,
                    "https://api.courselibrary.com/errors/gateway-timeout",
                    "Gateway timeout",
                    "The downstream request was cancelled or timed out."),

            _ =>
                new GatewayExceptionMapping(
                    StatusCodes.Status500InternalServerError,
                    "https://api.courselibrary.com/errors/internal-server-error",
                    "Internal server error",
                    "An unexpected gateway error occurred.")
        };
    }

    private void LogException(
        HttpContext context,
        Exception exception,
        int statusCode,
        string traceId)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Gateway request failed. StatusCode: {StatusCode}, TraceId: {TraceId}, Method: {Method}, Path: {Path}",
                statusCode,
                traceId,
                context.Request.Method,
                context.Request.Path);

            return;
        }

        logger.LogWarning(
            exception,
            "Gateway request failed. StatusCode: {StatusCode}, TraceId: {TraceId}, Method: {Method}, Path: {Path}",
            statusCode,
            traceId,
            context.Request.Method,
            context.Request.Path);
    }

    private static string GetTraceId(HttpContext context)
    {
        return Activity.Current?.TraceId.ToString()
            ?? context.TraceIdentifier;
    }

    private sealed record GatewayExceptionMapping(
        int StatusCode,
        string Type,
        string Title,
        string Detail);
}