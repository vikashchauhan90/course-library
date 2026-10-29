using System.Net.Mail;

namespace CourseLibrary.Idp.Infrastructure;

public sealed class SmtpEmailOptions
{
    public const string SectionName = "Email:Smtp";

    public string Host { get; set; } = "";

    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; }

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public string SenderAddress { get; set; } = "no-reply@example.com";

    public string SenderName { get; set; } = "Course Library";

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port is > 0 and <= 65535 &&
        MailAddress.TryCreate(SenderAddress, out _) &&
        (string.IsNullOrWhiteSpace(Username) == string.IsNullOrWhiteSpace(Password));
}
