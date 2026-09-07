using CourseLibrary.Api.Endpoints.Courses.DeleteCourse;
using CourseLibrary.Api.Endpoints.Courses.GetCourse;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using CourseLibrary.Api.Endpoints.Courses.RetireCourse;
using CourseLibrary.Api.Endpoints.Courses.UpdateCourse;
using CourseLibrary.Models;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Courses;

public class CourseHelper
{
    public static IResource<Models.Course.CourseResponse> GetCourseResponse(
        LinkGenerator linkGenerator,
        Models.Course.CourseResponse course,
        string version = "1")
    {
        return new ResourceBuilder<Models.Course.CourseResponse>(course)
                       .AddLink(
                           "self",
                           linkGenerator.GetPathByName(
                               GetCourseEndpoint.RouteName,
                               values: new { version = version, courseId = course.Id })!,
                           HttpVerbs.Get)
                       .AddLink(
                           "collection",
                           linkGenerator.GetPathByName(
                               GetCoursesEndpoint.RouteName,
                               values: new { version = version })!,
                           HttpVerbs.Get)
                        .AddLink(
                           "update",
                           linkGenerator.GetPathByName(
                               UpdateCourseEndpoint.RouteName,
                               values: new { version = version, courseId = course.Id })!,
                           HttpVerbs.Put)
                         .AddLink(
                           "delete",
                           linkGenerator.GetPathByName(
                               DeleteCourseEndpoint.RouteName,
                               values: new { version = version, courseId = course.Id })!,
                           HttpVerbs.Delete)
                         .AddLink(
                           "retire",
                           linkGenerator.GetPathByName(
                               RetireCourseEndpoint.RouteName,
                               values: new { version = version, courseId = course.Id })!,
                           HttpVerbs.Post)
                       .Build();
    }

    public static IResource<IResource<Models.Course.CourseResponse>[]> GetCoursesResponse(
   LinkGenerator linkGenerator,
   IEnumerable<Models.Course.CourseResponse> courses,
   string version = "1")
    {
        var resources = courses
            .Select(course => GetCourseResponse(linkGenerator, course, version))
            .ToArray();

        return new ResourceBuilder<IResource<Models.Course.CourseResponse>[]>(resources)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    values: new { version = version })!,
                HttpVerbs.Get)
            .Build();
    }

    public static IResource<PageResult<IResource<Models.Course.CourseResponse>>> GetCoursesResponse(
    LinkGenerator linkGenerator,
    PageResult<Models.Course.CourseResponse> page,
    string version = "1")
    {
        var response = page.Map(course => GetCourseResponse(linkGenerator, course, version));
        return new ResourceBuilder<PageResult<IResource<Models.Course.CourseResponse>>>(response)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    values: new { version = version })!,
                HttpVerbs.Get)
            .Build();
    }
    private static Models.Course.CourseResponse ToDetails(Models.Course.CourseResponse course)
        => new(
                course.Id,
                course.Title,
                course.Description,
                course.AuthorId,
                course.AuthorName,
                course.CreatedAt,
                course.UpdatedAt,
                course.RetiredAt,
                course.DeletedAt,
                course.Comments.Select(comment => new Models.Course.CommentResponse(
                    comment.Id,
                    comment.CourseId,
                    comment.AuthorId,
                    comment.Content,
                    comment.ParentCommentId,
                    comment.CreatedAt,
                    comment.UpdatedAt)).ToList(),
            course.Discussions.Select(discussion => new Models.Course.DiscussionResponse(
                    discussion.Id,
                    discussion.CourseId,
                    discussion.Title,
                    discussion.Description,
                    discussion.CreatedAt,
                    discussion.UpdatedAt)).ToList());
    }
