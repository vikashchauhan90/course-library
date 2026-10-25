using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Courses.Update;

internal sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseValidator()
    {
        RuleFor(request => request.Title).RequiredText();
        RuleFor(request => request.Description).RequiredText();
    }
}
