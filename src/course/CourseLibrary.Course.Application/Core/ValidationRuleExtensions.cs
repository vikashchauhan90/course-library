using FluentValidation;

namespace CourseLibrary.Course.Application.Core;

internal static class ValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, string?> RequiredText<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
        => ruleBuilder
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("{PropertyName} is required.");

    public static IRuleBuilderOptions<T, string?> Identifier<T>(
        this IRuleBuilderInitial<T, string?> ruleBuilder,
        string displayName)
        => ruleBuilder
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage($"{displayName} is required.")
            .Must(value => Guid.TryParse(value, out _))
            .WithMessage($"{displayName} must be a valid identifier.");

    public static IRuleBuilderOptions<T, string?> OptionalIdentifier<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string displayName)
        => ruleBuilder
            .Must(value => string.IsNullOrWhiteSpace(value) || Guid.TryParse(value, out _))
            .WithMessage($"{displayName} must be a valid identifier.");
}
