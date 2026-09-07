using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Discussions;

internal static class DiscussionHalHelper
{
    public static IResource<DiscussionResponse> ToResource(
        LinkGenerator linkGenerator,
        DiscussionResponse discussion,
        string version = "1")
    {
        return new ResourceBuilder<DiscussionResponse>(discussion)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    "GetDiscussion",
                    new { version, discussionId = discussion.Id, courseId = discussion.CourseId })!,
                HttpVerbs.Get)
            .AddLink(
                "courses",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    new { version })!,
                HttpVerbs.Get)
            .AddLink(
                "update",
                linkGenerator.GetPathByName(
                    "UpdateDiscussion",
                    new { version, discussionId = discussion.Id, courseId = discussion.CourseId })!,
                HttpVerbs.Put)
            .AddLink(
                "delete",
                linkGenerator.GetPathByName(
                    "DeleteDiscussion",
                    new { version, discussionId = discussion.Id, courseId = discussion.CourseId })!,
                HttpVerbs.Delete)
            .Build();
    }
}
