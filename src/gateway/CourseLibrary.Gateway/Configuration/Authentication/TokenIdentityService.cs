using System.Security.Claims;

namespace CourseLibrary.Gateway.Configuration.Authentication;

internal sealed class TokenIdentityService : ITokenIdentityService
{
    private const string SubjectClaim = "sub";
    private const string AuthorizedPartyClaim = "azp";

    // IdP-specific convention for M2M subjects:
    // course-service@clients
    private const string M2MSubjectSuffix = "@clients";

    public TokenIdentityType GetIdentityType(
        ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true)
            return TokenIdentityType.Unknown;

        var subject = GetSubjectId(principal);

        if (string.IsNullOrWhiteSpace(subject))
            return TokenIdentityType.Unknown;

        return IsM2MSubject(subject)
            ? TokenIdentityType.M2M
            : TokenIdentityType.User;
    }

    public bool IsM2M(ClaimsPrincipal principal)
    {
        return GetIdentityType(principal)
            == TokenIdentityType.M2M;
    }

    public bool IsUser(ClaimsPrincipal principal)
    {
        return GetIdentityType(principal)
            == TokenIdentityType.User;
    }

    public string? GetSubjectId(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirstValue(SubjectClaim);
    }

    public string? GetClientId(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        // Only M2M tokens should expose a client identity
        // through this service.
        if (!IsM2M(principal))
            return null;

        /*
         * For your token format:
         *
         * sub = course-service@clientid
         * azp = course-service
         *
         * azp is the preferred client identifier.
         */
        var authorizedParty =
            principal.FindFirstValue(AuthorizedPartyClaim);

        if (!string.IsNullOrWhiteSpace(authorizedParty))
            return authorizedParty;

        // Fallback to the subject-derived client ID.
        return ExtractClientIdFromSubject(
            GetSubjectId(principal));
    }

    private static bool IsM2MSubject(string subject)
    {
        return subject.EndsWith(
            M2MSubjectSuffix,
            StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractClientIdFromSubject(
        string? subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return null;

        if (!IsM2MSubject(subject))
            return null;

        var clientId = subject[..^M2MSubjectSuffix.Length];

        return string.IsNullOrWhiteSpace(clientId)
            ? null
            : clientId;
    }
}
