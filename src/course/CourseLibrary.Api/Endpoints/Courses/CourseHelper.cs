using CourseLibrary.Api.Endpoints.Courses.DeleteCourse;
using CourseLibrary.Api.Endpoints.Courses.GetCourse;
using CourseLibrary.Api.Endpoints.Courses.GetCourses;
using CourseLibrary.Api.Endpoints.Courses.UpdateCourse;
using CourseLibrary.Application.Operations.Courses;
using CourseLibrary.Domain.Models;
using Hal.Core;
using Hal.Core.Builders;

namespace CourseLibrary.Api.Endpoints.Courses;

public class CourseHelper
{
    public static IResource<CourseResponse> GetCourseResponse(
        LinkGenerator linkGenerator,
        CourseResponse course,
        string version = "1")
    {
        return new ResourceBuilder<CourseResponse>(course)
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

    public static IResource<IResource<CourseResponse>[]> GetCoursesResponse(
   LinkGenerator linkGenerator,
   IEnumerable<CourseResponse> courses,
   string version = "1")
    {
        var resources = courses
            .Select(course => GetCourseResponse(linkGenerator, course, version))
            .ToArray();

        return new ResourceBuilder<IResource<CourseResponse>[]>(resources)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    values: new { version = version })!,
                HttpVerbs.Get)
            .Build();
    }

    public static IResource<PageResult<IResource<CourseResponse>>> GetCoursesResponse(
    LinkGenerator linkGenerator,
    PageResult<CourseResponse> page,
    string version = "1")
    {
        var response = page.Map(course => GetCourseResponse(linkGenerator, course, version));
        return new ResourceBuilder<PageResult<IResource<CourseResponse>>>(response)
            .AddLink(
                "self",
                linkGenerator.GetPathByName(
                    GetCoursesEndpoint.RouteName,
                    values: new { version = version })!,
                HttpVerbs.Get)
            .Build();
    }
}
