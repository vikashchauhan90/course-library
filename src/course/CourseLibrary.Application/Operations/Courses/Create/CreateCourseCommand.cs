using CourseLibrary.Models.Course;
using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Create;

public sealed record CreateCourseCommand(
    string Title,
    string Description,
    string AuthorId,
    string AuthorName)
    : ICommand<CourseResponse>;
