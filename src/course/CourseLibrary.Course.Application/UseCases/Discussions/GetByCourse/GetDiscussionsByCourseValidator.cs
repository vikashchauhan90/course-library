using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.UseCases.Discussions.GetByCourse;

internal sealed record GetDiscussionsByCourseRequest(string? CourseId);

internal sealed class GetDiscussionsByCourseValidator
    : AbstractValidator<GetDiscussionsByCourseRequest>
{
    public GetDiscussionsByCourseValidator()
        => RuleFor(request => request.CourseId).Identifier("CourseId");
}
