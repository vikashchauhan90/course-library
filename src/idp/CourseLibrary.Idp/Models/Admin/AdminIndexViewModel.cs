namespace CourseLibrary.Idp.Models.Admin;

public sealed class AdminIndexViewModel
{
    public required IReadOnlyList<AdminUserItem> Users { get; init; }
    public required IReadOnlyList<AdminRoleItem> Roles { get; init; }
    public required IReadOnlyList<AdminPermissionItem> Permissions { get; init; }
    public required IReadOnlyList<AdminClientItem> Clients { get; init; }
    public required IReadOnlyList<AdminScopeItem> Scopes { get; init; }
}

public sealed record AdminUserItem(
    string Id,
    string UserName,
    string Email,
    string Status,
    bool IsLockedOut,
    bool IsDeactivated,
    bool CanSendPasswordReset,
    bool IsAdministrator,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset? PasswordChangedAt);
public sealed record AdminRoleItem(string Name, IReadOnlySet<string> Permissions);
public sealed record AdminPermissionItem(string Value, string DisplayName);
public sealed record AdminClientItem(
    string ClientId,
    string? DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SecretCreatedAt,
    DateTimeOffset? SecretRotatedAt,
    DateTimeOffset? SecretExpiresAt,
    bool IsSecretExpired,
    bool HasClientSecret,
    string AllowedScopes);
public sealed record AdminScopeItem(string Name, string? DisplayName, string Resources);
public sealed record AdminInvitationItem(
    string Id,
    string Email,
    string FullName,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? RevokedAt,
    string Status,
    bool CanResend,
    bool CanRevoke);
