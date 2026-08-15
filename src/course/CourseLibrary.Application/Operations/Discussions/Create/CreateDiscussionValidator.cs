using FluentValidation;

namespace CourseLibrary.Application.Operations.Discussions.Create;

public sealed class CreateDiscussionValidator : AbstractValidator<CreateDiscussionCommand>
{
    public CreateDiscussionValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
    }
}
