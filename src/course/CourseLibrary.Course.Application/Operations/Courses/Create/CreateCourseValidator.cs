using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Courses.Create;

internal sealed record CreateCourseOperationRequest(CreateCourseRequest? Request, string? ActorId);

internal sealed class CreateCourseValidator : AbstractValidator<CreateCourseOperationRequest>
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
