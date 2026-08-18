using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models;

public sealed class InviteUserViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(200)] public string FullName { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
}

public sealed class AcceptInvitationViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Token { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), StringLength(128, MinimumLength = 12)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
}
