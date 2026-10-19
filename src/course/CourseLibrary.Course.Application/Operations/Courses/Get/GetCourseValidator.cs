using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Courses.Get;

internal sealed record GetCourseRequest(string? CourseId);

internal sealed class GetCourseValidator : AbstractValidator<GetCourseRequest>
{
    public GetCourseValidator()
        => RuleFor(request => request.CourseId).Identifier("CourseId");
}
