using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Discussions.Create;

internal sealed record CreateDiscussionRequest(string? CourseId, string? Title, string? Description);

internal sealed class CreateDiscussionValidator : AbstractValidator<CreateDiscussionRequest>
{
    public CreateDiscussionValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.Title).RequiredText();
        RuleFor(request => request.Description).RequiredText();
    }
}
