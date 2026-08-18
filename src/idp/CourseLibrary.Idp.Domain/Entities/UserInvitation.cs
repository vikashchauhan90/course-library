namespace CourseLibrary.Idp.Domain.Entities;

/// <summary>Single-use invitation. Only a SHA-256 hash of the bearer token is stored.</summary>
public sealed class UserInvitation
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
