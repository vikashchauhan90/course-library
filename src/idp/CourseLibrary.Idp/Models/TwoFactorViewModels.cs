using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models;

public sealed class VerifyAuthenticatorViewModel
{
    [Required, StringLength(7, MinimumLength = 6)] public string Code { get; set; } = string.Empty;
    public bool RememberMachine { get; set; }
}

public sealed class RecoveryCodeViewModel
{
    [Required] public string Code { get; set; } = string.Empty;
}

public sealed class EnableAuthenticatorViewModel
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
    [Required, StringLength(7, MinimumLength = 6)] public string Code { get; set; } = string.Empty;
}
