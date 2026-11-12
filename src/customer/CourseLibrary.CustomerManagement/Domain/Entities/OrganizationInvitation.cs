namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class OrganizationInvitation
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string Role { get; set; }
    public required string InvitedByUserId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<OrganizationInvitationProduct> Products { get; set; } = [];
}
