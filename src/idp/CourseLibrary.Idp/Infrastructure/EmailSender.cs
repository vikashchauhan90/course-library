using CourseLibrary.Idp.Abstractions;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CourseLibrary.Idp.Infrastructure;

public sealed class EmailSender(
    IOptions<SmtpEmailOptions> smtpOptions,
    ILogger<EmailSender> logger) : IEmailSender
{
    public async Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var options = smtpOptions.Value;
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(options.SenderName, options.SenderAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        var socketOptions = options.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(options.Host, options.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(options.Username))
            await client.AuthenticateAsync(options.Username, options.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Email sent with subject {Subject}.", subject);
    }
}