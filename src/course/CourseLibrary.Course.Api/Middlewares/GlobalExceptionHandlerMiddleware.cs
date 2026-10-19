using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Course.Api.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CourseLibrary.Course.Api.Middlewares;

internal sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or the request was explicitly cancelled.
            // Do not attempt to write a response.
            logger.LogDebug(
                "Request was cancelled by the client. TraceId: {TraceId}, Path: {Path}",
                Activity.Current?.Id ?? context.TraceIdentifier,
                context.Request.Path);

            return;
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
            logger.LogDebug(
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
            Type = DomainExceptionExtensions.GetProblemType(error.StatusCode),
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        if (error.Errors is not null)
        {
            problemDetails.Extensions["errors"] = error.Errors;
        }

        try
        {
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(
             problemDetails,
             context.RequestAborted);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {

            logger.LogDebug(
                "Request was cancelled while writing the error response. TraceId: {TraceId}, Path: {Path} ",
                traceId,
                context.Request.Path);
        }
    }

    private static ExceptionMapping MapException(
         Exception exception)
    {
        return exception switch
        {
            DomainException baseException =>
            DomainExceptionExtensions.MapException(baseException),
            FluentValidation.ValidationException validationException =>
                new ExceptionMapping(
                    StatusCodes.Status422UnprocessableEntity,
                    "Validation failed",
                    "One or more validation errors occurred.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status422UnprocessableEntity),
                    validationException.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group
                                .Select(x => x.ErrorMessage)
                                .ToArray())),

            ValidationException =>
                new ExceptionMapping(
                    StatusCodes.Status422UnprocessableEntity,
                    "Validation failed",
                    "One or more validation errors occurred.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status422UnprocessableEntity)),

            ArgumentNullException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains an invalid argument.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status400BadRequest)),

            ArgumentOutOfRangeException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains an argument outside the allowed range.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status400BadRequest)),

            ArgumentException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains invalid arguments.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status400BadRequest)),

            FormatException =>
                new ExceptionMapping(
                    StatusCodes.Status400BadRequest,
                    "Bad request",
                    "The request contains an invalid format.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status400BadRequest)),

            KeyNotFoundException =>
                new ExceptionMapping(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "The requested resource was not found.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status404NotFound)),

            FileNotFoundException =>
                new ExceptionMapping(
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    "The requested resource was not found.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status404NotFound)),

            TimeoutException =>
                new ExceptionMapping(
                    StatusCodes.Status504GatewayTimeout,
                    "Gateway timeout",
                    "The downstream operation did not complete within the allowed time.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status504GatewayTimeout)),

            _ =>
                new ExceptionMapping(
                    StatusCodes.Status500InternalServerError,
                    "Internal server error",
                    "An unexpected error occurred.",
                    DomainExceptionExtensions.GetProblemType(
                        StatusCodes.Status500InternalServerError))
        };
    }
}