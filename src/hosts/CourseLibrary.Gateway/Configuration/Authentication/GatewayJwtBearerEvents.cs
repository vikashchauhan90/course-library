using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace CourseLibrary.Gateway.Configuration.Authentication;

internal sealed class GatewayJwtBearerEvents : JwtBearerEvents
{
    private readonly ILogger<GatewayJwtBearerEvents> _logger;
    private readonly ITokenIdentityService _tokenIdentityService;

    public GatewayJwtBearerEvents(
        ILogger<GatewayJwtBearerEvents> logger,
        ITokenIdentityService tokenIdentityService)
    {
        _logger = logger;
        _tokenIdentityService = tokenIdentityService;
    }

    public override Task AuthenticationFailed(
        AuthenticationFailedContext context)
    {
        _logger.LogWarning(
            context.Exception,
            "JWT authentication failed for {Path}.",
            context.HttpContext.Request.Path);

        return Task.CompletedTask;
    }

    public override Task TokenValidated(
        TokenValidatedContext context)
    {
        var principal = context.Principal;

        if (principal is null)
        {
            _logger.LogWarning(
                "JWT validation completed without a ClaimsPrincipal.");

            return Task.CompletedTask;
        }

        var identityType =
            _tokenIdentityService.GetIdentityType(principal);

        if (identityType == TokenIdentityType.M2M)
        {
            var clientId =
                _tokenIdentityService.GetClientId(principal);

            _logger.LogDebug(
                "M2M JWT successfully validated for client {ClientId}.",
                clientId);

            return Task.CompletedTask;
        }

        if (identityType == TokenIdentityType.User)
        {
            _logger.LogDebug(
                "User JWT successfully validated.");

            return Task.CompletedTask;
        }

        _logger.LogWarning(
            "JWT was cryptographically valid but its identity type could not be determined.");

        return Task.CompletedTask;
    }
}
