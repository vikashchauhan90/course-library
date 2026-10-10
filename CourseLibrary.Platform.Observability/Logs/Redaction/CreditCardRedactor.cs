using Microsoft.Extensions.Compliance.Redaction;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;

public sealed class CreditCardRedactor : Redactor
{
    public override int GetRedactedLength(ReadOnlySpan<char> input)
        => input.Length;

    public override int Redact(
        ReadOnlySpan<char> source,
        Span<char> destination)
        => DigitSuffixMaskingRedactor.Redact(source, destination, visibleSuffixDigits: 4);
}