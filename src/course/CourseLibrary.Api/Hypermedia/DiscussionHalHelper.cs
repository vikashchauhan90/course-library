using CourseLibrary.Application.Operations.Discussions;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Hypermedia;

internal static class DiscussionHalHelper
{
    public static IResource<DiscussionResponse> ToResource(
        LinkGenerator linkGenerator,
        DiscussionResponse discussion)
    {
        return new ResourceBuilder<DiscussionResponse>(discussion)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    "GetDiscussion",
                    new { version = "1", discussionId = discussion.Id, courseId = discussion.CourseId })!,
                HttpVerbs.Get)
            .AddLink(
                "course",
                linkGenerator.GetPathByName(
                    "GetCourse",
                    new { version = "1", courseId = discussion.CourseId, partitionKey = discussion.CourseId })!,
                HttpVerbs.Get)
            .AddLink(
                "update",
                linkGenerator.GetPathByName(
                    "UpdateDiscussion",
                    new { version = "1", discussionId = discussion.Id, courseId = discussion.CourseId })!,
                HttpVerbs.Put)
            .AddLink(
                "delete",
                linkGenerator.GetPathByName(
                    "DeleteDiscussion",
                    new { version = "1", discussionId = discussion.Id, courseId = discussion.CourseId })!,
                HttpVerbs.Delete)
            .Build();
    }
}
