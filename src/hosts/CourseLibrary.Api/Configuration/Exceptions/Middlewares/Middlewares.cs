using CourseLibrary.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CourseLibrary.Api.Configuration.Exceptions.Middlewares;

public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        // Client disconnected / request was cancelled.
        // Do not try to write a response because the client may no longer exist.
        if (exception is OperationCanceledException &&
            context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "Request was cancelled by the client. TraceId: {TraceId}, Path: {Path}",
                Activity.Current?.Id ?? context.TraceIdentifier,
                context.Request.Path);

            return;
        }

        var error = MapException(exception);

        var traceId =
            Activity.Current?.Id ??
            context.TraceIdentifier;

        // Don't log expected 4xx exceptions as errors.
        if (error.StatusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception. StatusCode: {StatusCode}, TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                error.StatusCode,
                traceId,
                context.Request.Path,
                context.Request.Method);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed. StatusCode: {StatusCode}, TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                error.StatusCode,
                traceId,
                context.Request.Path,
                context.Request.Method);
        }

        if (context.Response.HasStarted)
        {
            logger.LogWarning(
                "Cannot write exception response because the response has already started. TraceId: {TraceId}",
                traceId);

            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = error.StatusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Message,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        if (error.Errors is not null)
        {
            problemDetails.Extensions["errors"] = error.Errors;
        }

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            context.RequestAborted);
    }

    private static ExceptionMapping MapException(Exception exception)
    {
        return exception switch
        {
            BaseException baseException =>
            new ExceptionMapping(
                baseException.StatusCode,
                GetTitle(baseException.StatusCode),
                baseException.Message),

            // FluentValidation
            FluentValidation.ValidationException validationException =>
                new ExceptionMapping(
                    StatusCodes.Status422UnprocessableEntity,
                    "Validation failed",
                    "One or more validation errors occurred.",
                    validationException.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ErrorMessage).ToArray())),

            // ASP.NET / DataAnnotations validation
            ValidationException dataAnnotationsException =>
                new ExceptionMapping(
                    StatusCodes.Status422UnprocessableEntity,
                    "Validation failed",
                    dataAnnotationsException.Message),

            // Bad arguments
            ArgumentNullException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains an invalid argument."),

            ArgumentOutOfRangeException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains an argument outside the allowed range."),

            ArgumentException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains invalid arguments."),

            FormatException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains an invalid format."),

            // Resource not found
            KeyNotFoundException =>
                new ExceptionMapping(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "The requested resource was not found."),

            FileNotFoundException =>
                new ExceptionMapping(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "The requested resource was not found."),

            // TODO Authorization exceptions (e.g., UnauthorizedAccessException) can be handled here as well.
            // Authentication / authorization
            UnauthorizedAccessException =>
                new ExceptionMapping(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "Authentication is required to access this resource."),


            // Conflict
            InvalidOperationException =>
                new ExceptionMapping(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "The requested operation could not be completed because of a conflict."),

            // Rate limiting
            _ when IsTooManyRequests(exception) =>
                new ExceptionMapping(
                    StatusCodes.Status429TooManyRequests,
                    "Too many requests",
                    "Too many requests were sent. Please try again later."),

            // Timeout
            TimeoutException =>
                new ExceptionMapping(
                    StatusCodes.Status504GatewayTimeout,
                    "Request timeout",
                    "The request could not be completed within the allowed time."),

            // Server-side cancellation / timeout
            OperationCanceledException =>
                new ExceptionMapping(
                    StatusCodes.Status408RequestTimeout,
                    "Request timeout",
                    "The request was cancelled or timed out."),

            // Everything else
            _ =>
                new ExceptionMapping(
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred.")
        };
    }

    private static bool IsTooManyRequests(Exception exception)
    {
        // Replace this with your actual rate-limit exception
        // if you have one.
        return exception.GetType().Name is
            "TooManyRequestsException" or
            "RateLimitExceededException";
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Resource not found",
            409 => "Conflict",
            422 => "Validation failed",
            429 => "Too many requests",
            408 => "Request timeout",
            502 => "Bad gateway",
            503 => "Service unavailable",
            504 => "Gateway timeout",
            _ when statusCode >= 500 => "Internal server error",
            _ => "Request failed"
        };
    }

    private sealed record ExceptionMapping(
        int StatusCode,
        string Title,
        string Message,
        object? Errors = null);
}