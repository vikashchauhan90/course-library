using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.CustomerManagement.Models;

public sealed class CreateInvitationViewModel
{
    public Guid OrganizationId { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Member";

    public string OrganizationName { get; set; } = string.Empty;
}

public sealed class AcceptInvitationViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    public string OrganizationName { get; set; } = string.Empty;
    public string InvitationEmail { get; set; } = string.Empty;
    public string? SignedInEmail { get; set; }
    public string? Error { get; set; }
}
