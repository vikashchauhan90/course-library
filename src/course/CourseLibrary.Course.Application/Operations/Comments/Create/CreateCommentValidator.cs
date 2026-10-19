using CourseLibrary.Course.Application.Core;
using FluentValidation;

namespace CourseLibrary.Course.Application.Operations.Comments.Create;

internal sealed record CreateCommentOperationRequest(
    string? CourseId,
    string? AuthorId,
    string? Content,
    string? ParentCommentId);

internal sealed class CreateCommentValidator : AbstractValidator<CreateCommentOperationRequest>
{
    public CreateCommentValidator()
    {
        RuleFor(request => request.CourseId).Identifier("CourseId");
        RuleFor(request => request.AuthorId).RequiredText();
        RuleFor(request => request.Content).RequiredText();
        RuleFor(request => request.ParentCommentId).OptionalIdentifier("ParentCommentId");
    }
}
