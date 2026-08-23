using CourseLibrary.Idp.Domain.Abstractions;

namespace CourseLibrary.Idp.Domain.Entities;

public sealed class UserInvitation : IEntity<string>, IEntityAudit, IEntityConcurrency
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ConcurrencyStamp { get; set; }
}
