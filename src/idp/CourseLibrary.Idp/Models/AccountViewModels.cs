using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.Idp.Models;

public sealed class RegisterViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}

public sealed class EmailViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}

public sealed class ResetPasswordViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Token { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string NewPassword { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))] public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ChangeEmailViewModel
{
    [Required, EmailAddress] public string NewEmail { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}

public sealed class ProfileViewModel
{
    [Required, StringLength(100, MinimumLength = 2)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string? UserName { get; set; }
}

public sealed class ExternalLoginItem
{
    public string LoginProvider { get; init; } = string.Empty;
    public string ProviderKey { get; init; } = string.Empty;
    public string ProviderDisplayName { get; init; } = string.Empty;
}

public sealed class DeleteAccountViewModel
{
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
}