using CourseLibrary.Application.Operations.Comments;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Hypermedia;

internal static class CommentHalHelper
{
    public static IResource<CommentResponse> ToResource(
        LinkGenerator linkGenerator,
        CommentResponse comment)
    {
        return new ResourceBuilder<CommentResponse>(comment)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    "GetComment",
                    new { version = "1", commentId = comment.Id, courseId = comment.CourseId })!,
                HttpVerbs.Get)
            .AddLink(
                "course",
                linkGenerator.GetPathByName(
                    "GetCourse",
                    new { version = "1", courseId = comment.CourseId, partitionKey = comment.CourseId })!,
                HttpVerbs.Get)
            .AddLink(
                "update",
                linkGenerator.GetPathByName(
                    "UpdateComment",
                    new { version = "1", commentId = comment.Id, courseId = comment.CourseId })!,
                HttpVerbs.Put)
            .AddLink(
                "delete",
                linkGenerator.GetPathByName(
                    "DeleteComment",
                    new { version = "1", commentId = comment.Id, courseId = comment.CourseId })!,
                HttpVerbs.Delete)
            .Build();
    }
}
