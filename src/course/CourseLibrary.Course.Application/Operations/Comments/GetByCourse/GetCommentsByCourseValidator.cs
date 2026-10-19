using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Comments.GetByCourse;

internal sealed record GetCommentsByCourseRequest(string? CourseId);

internal sealed class GetCommentsByCourseValidator : AbstractValidator<GetCommentsByCourseRequest>
{
    public GetCommentsByCourseValidator()
        => RuleFor(request => request.CourseId).Identifier("CourseId");
}
