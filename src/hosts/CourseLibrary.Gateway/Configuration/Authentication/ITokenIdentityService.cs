using System.Security.Claims;

namespace CourseLibrary.Gateway.Configuration.Authentication;

public interface ITokenIdentityService
{
    TokenIdentityType GetIdentityType(ClaimsPrincipal principal);

    bool IsUser(ClaimsPrincipal principal);

    bool IsM2M(ClaimsPrincipal principal);

    string? GetSubjectId(ClaimsPrincipal principal);

    string? GetClientId(ClaimsPrincipal principal);
}