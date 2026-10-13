using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Discussions.Update;

internal sealed record UpdateDiscussionRequest(
    string? DiscussionId,
    string? CourseId,
    string? Title,
    string? Description);

internal sealed class UpdateDiscussionValidator : AbstractValidator<UpdateDiscussionRequest>
{
    public UpdateDiscussionValidator()
    {
        RuleFor(request => request.DiscussionId).Identifier("DiscussionId");
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.Title).RequiredText();
        RuleFor(request => request.Description).RequiredText();
    }
}
