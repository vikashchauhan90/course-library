using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.Gateway.Configuration.Authentication;

internal sealed class GatewayJwtBearerEvents(
    ILogger<GatewayJwtBearerEvents> logger,
    ITokenIdentityService tokenIdentityService)
    : JwtBearerEvents
{
    public override Task AuthenticationFailed(
        AuthenticationFailedContext context)
    {
        logger.LogWarning(
            context.Exception,
            "JWT authentication failed. TraceId: {TraceId}, Path: {Path}",
            GetTraceId(context.HttpContext),
            context.HttpContext.Request.Path);

        return Task.CompletedTask;
    }

    public override async Task Challenge(
        JwtBearerChallengeContext context)
    {
        logger.LogWarning(
            "JWT authentication challenge. TraceId: {TraceId}, Path: {Path}",
            GetTraceId(context.HttpContext),
            context.HttpContext.Request.Path);

        // Prevent the default WWW-Authenticate/plain response.
        context.HandleResponse();

        await WriteProblemDetailsAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            "https://api.courselibrary.com/errors/unauthorized",
            "Unauthorized",
            "Authentication is required.");
    }

    public override async Task Forbidden(
        ForbiddenContext context)
    {
        logger.LogWarning(
            "JWT authorization forbidden. TraceId: {TraceId}, Path: {Path}",
            GetTraceId(context.HttpContext),
            context.HttpContext.Request.Path);

        await WriteProblemDetailsAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            "https://api.courselibrary.com/errors/forbidden",
            "Forbidden",
            "You do not have permission to access this resource.");
    }

    public override Task TokenValidated(
        TokenValidatedContext context)
    {
        var principal = context.Principal;

        if (principal is null)
        {
            logger.LogWarning(
                "JWT validation completed without a ClaimsPrincipal. TraceId: {TraceId}",
                GetTraceId(context.HttpContext));

            return Task.CompletedTask;
        }

        var identityType =
            tokenIdentityService.GetIdentityType(principal);

        switch (identityType)
        {
            case TokenIdentityType.M2M:
                {
                    var clientId =
                        tokenIdentityService.GetClientId(principal);

                    logger.LogDebug(
                        "M2M JWT successfully validated. ClientId: {ClientId}, TraceId: {TraceId}",
                        clientId,
                        GetTraceId(context.HttpContext));

                    break;
                }

            case TokenIdentityType.User:

                logger.LogDebug(
                    "User JWT successfully validated. TraceId: {TraceId}",
                    GetTraceId(context.HttpContext));

                break;

            default:

                logger.LogWarning(
                    "JWT was cryptographically valid but its identity type could not be determined. TraceId: {TraceId}",
                    GetTraceId(context.HttpContext));

                break;
        }

        return Task.CompletedTask;
    }

    private static string GetTraceId(HttpContext context)
    {
        return Activity.Current?.TraceId.ToString()
            ?? context.TraceIdentifier;
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string type,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            GetTraceId(context);

        problemDetails.Extensions["timestamp"] =
            DateTimeOffset.UtcNow;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            context.RequestAborted);
    }
}