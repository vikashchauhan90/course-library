using FluentValidation;

namespace CourseLibrary.Application.Operations.Comments.Create;

public sealed class CreateCommentValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.AuthorId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required");
    }
}
