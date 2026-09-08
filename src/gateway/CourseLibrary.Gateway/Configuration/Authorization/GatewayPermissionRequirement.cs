using Microsoft.AspNetCore.Authorization;

namespace CourseLibrary.Gateway.Configuration.Authorization;

internal sealed record GatewayPermissionRequirement(
    string Resource,
    string Action) : IAuthorizationRequirement;

internal sealed class AdministratorRequirement : IAuthorizationRequirement;