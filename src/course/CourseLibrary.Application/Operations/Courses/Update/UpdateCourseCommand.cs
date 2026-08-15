using MediatorForge.Abstractions;

namespace CourseLibrary.Application.Operations.Courses.Update;

public sealed record UpdateCourseCommand(string Id, string Title, string Description, string AuthorId) : ICommand<Domain.Entities.Course>;
