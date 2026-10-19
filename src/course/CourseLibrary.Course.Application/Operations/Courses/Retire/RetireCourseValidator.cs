using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Courses.Retire;

internal sealed record RetireCourseRequest(string? CourseId, string? ActorId);

internal sealed class RetireCourseValidator : AbstractValidator<RetireCourseRequest>
{
    public RetireCourseValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.ActorId).RequiredText();
    }
}
