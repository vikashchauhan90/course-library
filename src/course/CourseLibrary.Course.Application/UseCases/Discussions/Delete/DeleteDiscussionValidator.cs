using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Discussions.Delete;

internal sealed record DeleteDiscussionRequest(string? DiscussionId, string? CourseId);

internal sealed class DeleteDiscussionValidator : AbstractValidator<DeleteDiscussionRequest>
{
    public DeleteDiscussionValidator()
    {
        RuleFor(request => request.DiscussionId).Identifier("DiscussionId");
        RuleFor(request => request.CourseId).Identifier("CourseId");
    }
}
