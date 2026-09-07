using CourseLibrary.Api.Endpoints.Courses.DeleteCourse;
using CourseLibrary.Api.Endpoints.Courses.GetCourse;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using CourseLibrary.Api.Endpoints.Courses.UpdateCourse;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Models;
using CourseLibrary.Models.Course;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Courses;

public class CourseHelper
{
    public static IResource<CourseDetails> GetCourseResponse(
        LinkGenerator linkGenerator,
        CourseResponse course,
        string version = "1")
    {
        var details = ToDetails(course);
        return new ResourceBuilder<CourseDetails>(details)
                       .AddLink(
                           "self",
                           linkGenerator.GetPathByName(
                               GetCourseEndpoint.RouteName,
                               values: new { version = version, courseId = course.Id, partitionKey = course.AuthorId })!,
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
                               values: new { version = version, courseId = course.Id, partitionKey = course.AuthorId })!,
                           HttpVerbs.Put)
                         .AddLink(
                           "delete",
                           linkGenerator.GetPathByName(
                               DeleteCourseEndpoint.RouteName,
                               values: new { version = version, courseId = course.Id, partitionKey = course.AuthorId })!,
                           HttpVerbs.Delete)
                       .Build();
    }

    public static IResource<IResource<CourseDetails>[]> GetCoursesResponse(
   LinkGenerator linkGenerator,
   IEnumerable<CourseResponse> courses,
   string version = "1")
    {
        var resources = courses
            .Select(course => GetCourseResponse(linkGenerator, course, version))
            .ToArray();

        return new ResourceBuilder<IResource<CourseDetails>[]>(resources)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    values: new { version = version })!,
                HttpVerbs.Get)
            .Build();
    }

    public static IResource<PageResult<IResource<CourseDetails>>> GetCoursesResponse(
    LinkGenerator linkGenerator,
    PageResult<CourseResponse> page,
    string version = "1")
    {
        var response = page.Map(course => GetCourseResponse(linkGenerator, course, version));
        return new ResourceBuilder<PageResult<IResource<CourseDetails>>>(response)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    values: new { version = version })!,
                HttpVerbs.Get)
            .Build();
    }
    private static CourseDetails ToDetails(CourseResponse course)
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
                course.Comments.Select(comment => new CommentDetails(
                    comment.Id,
                    comment.CourseId,
                    comment.AuthorId,
                    comment.Content,
                    comment.ParentCommentId,
                    comment.CreatedAt,
                    comment.UpdatedAt)).ToList(),
            course.Discussions.Select(discussion => new DiscussionDetails(
                    discussion.Id,
                    discussion.CourseId,
                    discussion.Title,
                    discussion.Description,
                    discussion.CreatedAt,
                    discussion.UpdatedAt)).ToList());
    }
