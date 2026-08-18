using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models.Admin;

public sealed class CreateUserViewModel
{
    [Required, StringLength(100)] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(200)] public string FullName { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), StringLength(128, MinimumLength = 12)] public string Password { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
}
