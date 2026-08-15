using FluentValidation;

namespace CourseLibrary.Application.Operations.Authors.Create;

public sealed class CreateAuthorValidator : AbstractValidator<CreateAuthorCommand>
{
    public CreateAuthorValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}
