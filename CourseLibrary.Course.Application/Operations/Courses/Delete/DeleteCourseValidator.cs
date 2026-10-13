using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Courses.Delete;

internal sealed record DeleteCourseRequest(string? CourseId, string? ActorId);

internal sealed class DeleteCourseValidator : AbstractValidator<DeleteCourseRequest>
{
    public DeleteCourseValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.ActorId).RequiredText();
    }
}
