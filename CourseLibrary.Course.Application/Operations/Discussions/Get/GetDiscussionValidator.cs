using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Discussions.Get;

internal sealed record GetDiscussionRequest(string? DiscussionId, string? CourseId);

internal sealed class GetDiscussionValidator : AbstractValidator<GetDiscussionRequest>
{
    public GetDiscussionValidator()
    {
        RuleFor(request => request.DiscussionId).Identifier("DiscussionId");
        RuleFor(request => request.CourseId).Identifier("CourseId");
    }
}
