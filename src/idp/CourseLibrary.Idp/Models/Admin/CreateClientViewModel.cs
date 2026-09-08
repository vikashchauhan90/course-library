using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models.Admin;

public sealed class CreateClientViewModel
{
    [RegularExpression("^[A-Za-z0-9._-]{3,100}$")] public string? ClientId { get; set; }
    [Required, StringLength(200)] public string DisplayName { get; set; } = string.Empty;
    public bool IsMachineClient { get; set; }
    public bool AllowClientCredentials { get; set; }
    public bool AllowAuthorizationCode { get; set; }
    [Url] public string? RedirectUri { get; set; }
    public List<string> AllowedScopes { get; set; } = [];
    public IReadOnlyList<AdminScopeOption> AvailableScopes { get; set; } = [];
}

public sealed record AdminScopeOption(string Name, string? DisplayName, string Resources);
