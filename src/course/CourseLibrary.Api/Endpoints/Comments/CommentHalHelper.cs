using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using CourseLibrary.Models.Course;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Comments;

internal static class CommentHalHelper
{
    public static IResource<CommentDetails> ToResource(
        LinkGenerator linkGenerator,
        CommentResponse comment,
        string version = "1")
    {
            var details = new CommentDetails(
                comment.Id,
                comment.CourseId,
                comment.AuthorId,
                comment.Content,
                comment.ParentCommentId,
                comment.CreatedAt,
                comment.UpdatedAt);

            return new ResourceBuilder<CommentDetails>(details)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    "GetComment",
                    new { version, commentId = comment.Id, courseId = comment.CourseId })!,
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
                    "UpdateComment",
                    new { version, commentId = comment.Id, courseId = comment.CourseId })!,
                HttpVerbs.Put)
            .AddLink(
                "delete",
                linkGenerator.GetPathByName(
                    "DeleteComment",
                    new { version, commentId = comment.Id, courseId = comment.CourseId })!,
                HttpVerbs.Delete)
            .Build();
    }
}
