using CourseLibrary.Course.Application.Core;
using CourseLibrary.Course.Contracts.DTOs.Courses.Discussions;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Create;

internal sealed class CreateDiscussionValidator : AbstractValidator<DiscussionRequest>
{
    public CreateDiscussionValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.Title).RequiredText();
        RuleFor(request => request.Description).RequiredText();
    }
}
