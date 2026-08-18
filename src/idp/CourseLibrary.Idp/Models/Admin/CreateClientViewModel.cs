using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models.Admin;

public sealed class CreateClientViewModel
{
    [Required, RegularExpression("^[A-Za-z0-9._-]{3,100}$")] public string ClientId { get; set; } = string.Empty;
    [Required, StringLength(200)] public string DisplayName { get; set; } = string.Empty;
    public bool AllowClientCredentials { get; set; } = true;
    public bool AllowAuthorizationCode { get; set; }
    [Url] public string? RedirectUri { get; set; }
}
