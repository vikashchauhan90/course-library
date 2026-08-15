using Asp.Versioning;
using Carter;

namespace CourseLibrary.Api.Endpoints.Logging;

public sealed class LoggingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}")
            .WithTags("Logging");

        group.MapGet("/test-log", async (ILogger<LoggingModule> logger) =>
        {
            var email = "vikash.chauhan@gmail.com";
            await Task.Delay(TimeSpan.FromSeconds(1));
            logger.UserLoggedIn(email);
            logger.LogInformation(
                "This is testing message with user {UserId}",
                100);

            return Results.Ok("Log written.");
        })
        .WithName("TestLogV1")
        .WithTags("Logging")
        .HasApiVersion(1.0);

        group.MapGet("/test-log", async (ILogger<LoggingModule> logger) =>
        {
            var email = "vikash.chauhan@gmail.com";
            await Task.Delay(TimeSpan.FromSeconds(1));
            logger.LogInformation("This is version 2 logging test for {UserId}", 200);
            logger.UserLoggedIn(email);

            return Results.Ok("Log written from v2.");
        })
        .WithName("TestLogV2")
        .WithTags("Logging")
        .HasApiVersion(2.0);
    }
}
