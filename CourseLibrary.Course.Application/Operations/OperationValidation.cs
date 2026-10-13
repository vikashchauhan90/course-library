using CourseLibrary.Core.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace CourseLibrary.Course.Application.Operations;

internal enum OperationFieldType
{
    Required,
    Identifier,
    OptionalIdentifier,
    PositiveInteger
}

internal sealed record OperationValidationField(
    string Name,
    string? Value,
    OperationFieldType Type);

internal sealed record OperationValidationInput(
    IReadOnlyList<OperationValidationField> Fields);

internal sealed class OperationValidationInputValidator : AbstractValidator<OperationValidationInput>
{
    public OperationValidationInputValidator()
    {
        RuleForEach(input => input.Fields).ChildRules(field =>
        {
            field.RuleFor(item => item.Value)
                .NotEmpty()
                .When(item => item.Type is OperationFieldType.Required or OperationFieldType.Identifier)
                .WithMessage(item => $"'{item.Name}' is required.");

            field.RuleFor(item => item.Value)
                .Must(value => Guid.TryParse(value, out _))
                .When(item => item.Type == OperationFieldType.Identifier
                    || item.Type == OperationFieldType.OptionalIdentifier && !string.IsNullOrWhiteSpace(item.Value))
                .WithMessage(item => $"'{item.Name}' must be a valid identifier.");

            field.RuleFor(item => item.Value)
                .Must(value => int.TryParse(value, out var parsed) && parsed > 0)
                .When(item => item.Type == OperationFieldType.PositiveInteger)
                .WithMessage(item => $"'{item.Name}' must be greater than zero.");
        });
    }
}

internal static class OperationValidation
{
    public static async Task ValidateAsync(
        IValidator<OperationValidationInput> validator,
        CancellationToken cancellationToken,
        params OperationValidationField[] fields)
    {
        var input = new OperationValidationInput(fields);
        var result = await validator.ValidateAsync(input, cancellationToken);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(error => GetFieldName(error, input))
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        throw new ValidationFailedException
        {
            Errors = errors
        };
    }

    public static OperationValidationField Required(string name, string? value)
        => new(name, value, OperationFieldType.Required);

    public static OperationValidationField Identifier(string name, string? value)
        => new(name, value, OperationFieldType.Identifier);

    public static OperationValidationField OptionalIdentifier(string name, string? value)
        => new(name, value, OperationFieldType.OptionalIdentifier);

    public static OperationValidationField PositiveInteger(string name, int value)
        => new(name, value.ToString(System.Globalization.CultureInfo.InvariantCulture), OperationFieldType.PositiveInteger);

    private static string GetFieldName(ValidationFailure error, OperationValidationInput input)
    {
        const string prefix = "Fields[";
        var start = error.PropertyName.IndexOf(prefix, StringComparison.Ordinal);
        var end = start < 0 ? -1 : error.PropertyName.IndexOf(']', start + prefix.Length);
        if (start >= 0
            && end > start
            && int.TryParse(error.PropertyName.AsSpan(start + prefix.Length, end - start - prefix.Length), out var index)
            && index >= 0
            && index < input.Fields.Count)
        {
            return input.Fields[index].Name;
        }

        return error.PropertyName;
    }
}
