using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Courses.Update;

internal sealed record UpdateCourseUseCaseRequest(
    string? CourseId,
    UpdateCourseRequest? Request,
    string? ActorId);

internal sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseUseCaseRequest>
{
    public UpdateCourseValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.Request).NotNull();
        RuleFor(request => request.Request!.Title)
            .RequiredText()
            .When(request => request.Request is not null);
        RuleFor(request => request.Request!.Description)
            .RequiredText()
            .When(request => request.Request is not null);
        RuleFor(request => request.ActorId).RequiredText();
    }
}
