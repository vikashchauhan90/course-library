using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models.Admin;

public sealed class CreateScopeViewModel
{
    [Required, RegularExpression("^[A-Za-z0-9._-]{3,100}$")] public string Name { get; set; } = string.Empty;
    [Required, StringLength(200)] public string DisplayName { get; set; } = string.Empty;
    [Required, RegularExpression("^[A-Za-z0-9._-]{3,100}$")] public string Resource { get; set; } = string.Empty;
}
