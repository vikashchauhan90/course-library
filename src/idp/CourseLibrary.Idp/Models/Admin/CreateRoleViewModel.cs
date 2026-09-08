using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models.Admin;

public sealed class CreateRoleViewModel
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}