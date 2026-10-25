using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Courses.Create;

internal sealed record CreateCourseUseCaseRequest(CourseRequest? Request, string? ActorId);

internal sealed class CreateCourseValidator : AbstractValidator<CreateCourseUseCaseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request!.Title)
            .RequiredText()
            .When(request => request.Request is not null);
        RuleFor(request => request.Request!.Description)
            .RequiredText()
            .When(request => request.Request is not null);
        RuleFor(request => request.Request!.AuthorId)
            .RequiredText()
            .When(request => request.Request is not null);
        RuleFor(request => request.Request!.AuthorName)
            .RequiredText()
            .When(request => request.Request is not null);
        RuleFor(request => request.ActorId).RequiredText();
    }
}
