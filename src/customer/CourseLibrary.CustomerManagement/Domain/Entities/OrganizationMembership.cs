namespace CourseLibrary.CustomerManagement.Domain.Entities;

public sealed class OrganizationMembership
{
    public Guid OrganizationId { get; set; }
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public Organization? Organization { get; set; }
}
