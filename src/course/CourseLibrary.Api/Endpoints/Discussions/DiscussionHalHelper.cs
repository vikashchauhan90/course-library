using CourseLibrary.Application.Operations.Discussions;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using CourseLibrary.Models.Course;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Discussions;

internal static class DiscussionHalHelper
{
    public static IResource<Models.Course.DiscussionResponse> ToResource(
        LinkGenerator linkGenerator,
        Models.Course.DiscussionResponse discussion,
        string version = "1")
    {
            var details = new Models.Course.DiscussionResponse(
                discussion.Id,
                discussion.CourseId,
                discussion.Title,
                discussion.Description,
                discussion.CreatedAt,
                discussion.UpdatedAt);

            return new ResourceBuilder<Models.Course.DiscussionResponse>(details)
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
