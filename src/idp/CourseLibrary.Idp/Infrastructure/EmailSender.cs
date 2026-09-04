using CourseLibrary.Idp.Abstractions;

namespace CourseLibrary.Idp.Infrastructure;

public sealed class EmailSender(ILogger<EmailSender> logger) : IEmailSender
{
    public Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email queued for {Recipient}: {Subject}. Link/body: {Body}", recipient, subject, body);
        return Task.CompletedTask;
    }
}