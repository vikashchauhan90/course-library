using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Courses.Create;

internal sealed class CreateCourseValidator : AbstractValidator<CourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(request => request.Title).RequiredText();
        RuleFor(request => request.Description).RequiredText();
        RuleFor(request => request.AuthorId).RequiredText();
        RuleFor(request => request.AuthorName).RequiredText();
    }
}
