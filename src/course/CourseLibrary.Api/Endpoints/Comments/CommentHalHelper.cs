using CourseLibrary.Application.Operations.Comments;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Comments;

internal static class CommentHalHelper
{
    public static IResource<CommentResponse> ToResource(
        LinkGenerator linkGenerator,
        CommentResponse comment,
        string version = "1")
    {
        return new ResourceBuilder<CommentResponse>(comment)
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
