using Asp.Versioning;
using Asp.Versioning.Conventions;
using Carter;
using CourseLibrary.Api.Configuration;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Logging;

public sealed class LoggingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapApiVersionedGroup("/logging");

        group.MapGet(
            "/",
            async (
                LinkGenerator linkGenerator,
                ILogger<LoggingModule> logger) =>
            {
                var email = "vikash.chauhan@gmail.com";

                await Task.Delay(TimeSpan.FromSeconds(1));

                logger.UserLoggedIn(email);

                logger.LogInformation(
                    "This is testing message with user {UserId}",
                    100);

                var response = new ResourceBuilder<string>("Log written.")
                    .AddLink(
                        new Link
                        {
                            Rel = "self",
                            Href = linkGenerator.GetPathByName(
                                "TestLogV1",
                                values: new { version = "1" })!
                        })
                    .Build();

                return Results.Ok(response);
            })
            .WithName("TestLogV1");
    }
}