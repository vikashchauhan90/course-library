using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models.Admin;

public sealed class EditUserViewModel
{
    [Required] public string Id { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
    public bool IsLocked { get; set; }
}