using CourseLibrary.Core.Domain.Exceptions;
using CourseLibrary.Course.Api.Http;
using CourseLibrary.Course.Api.Observability;
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
            logger.RequestCancelled(
                Activity.Current?.Id ?? context.TraceIdentifier,
                context.Request.Path.Value,
                context.Request.Method);
            throw;
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                logger.FailureAfterResponseStarted(
                    exception,
                    Activity.Current?.Id ?? context.TraceIdentifier,
                    context.Request.Path.Value);
                throw;
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var error = MapException(exception);
        var traceId =
            Activity.Current?.Id ??
            context.TraceIdentifier;

        Activity.Current?.SetTag("error.type", exception.GetType().Name);
        if (error.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            Activity.Current?.SetStatus(
                ActivityStatusCode.Error,
                $"HTTP {error.StatusCode}");
        }

        if (error.StatusCode >= 500)
        {
            logger.UnhandledRequestFailed(
                exception,
                error.StatusCode,
                traceId,
                context.Request.Path.Value,
                context.Request.Method);
        }
        else
        {
            logger.RequestRejected(
                exception,
                error.StatusCode,
                traceId,
                context.Request.Path.Value,
                context.Request.Method);
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
            await context.Response.WriteAsJsonAsync(
                problemDetails,
                context.RequestAborted);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            logger.ErrorResponseWriteCancelled(
                traceId,
                context.Request.Path.Value);
            throw;
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
                    "The downstream useCase did not complete within the allowed time.",
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