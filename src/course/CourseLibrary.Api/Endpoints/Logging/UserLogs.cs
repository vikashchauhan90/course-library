using CourseLibrary.Infrastructure.Observability.Logs.Redaction;

namespace CourseLibrary.Api.Endpoints.Logging;

public static partial class UserLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "User email: {Email}")]
    public static partial void UserLoggedIn(
        this ILogger logger,

        [Email]
        string email);
}
