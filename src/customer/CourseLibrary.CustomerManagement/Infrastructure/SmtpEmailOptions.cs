using System.Net.Mail;

namespace CourseLibrary.CustomerManagement.Infrastructure;

public sealed class SmtpEmailOptions
{
    public const string SectionName = "Email:Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = "no-reply@example.com";
    public string SenderName { get; set; } = "Course Library";

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port is > 0 and <= 65535 &&
        MailAddress.TryCreate(SenderAddress, out _) &&
        (string.IsNullOrWhiteSpace(Username) == string.IsNullOrWhiteSpace(Password));
}
