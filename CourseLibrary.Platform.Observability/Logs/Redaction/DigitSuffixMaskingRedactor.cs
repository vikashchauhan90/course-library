using Microsoft.Extensions.Compliance.Redaction;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;

internal static class DigitSuffixMaskingRedactor
{
    public static int Redact(
        ReadOnlySpan<char> source,
        Span<char> destination,
        int visibleSuffixDigits)
    {
        var digitCount = 0;
        foreach (var character in source)
        {
            if (char.IsDigit(character))
            {
                digitCount++;
            }
        }

        if (digitCount == 0)
        {
            destination[..source.Length].Fill('*');
            return source.Length;
        }

        var digitsToMask = Math.Max(0, digitCount - visibleSuffixDigits);
        if (digitCount <= visibleSuffixDigits)
        {
            digitsToMask = digitCount;
        }

        for (var index = 0; index < source.Length; index++)
        {
            var character = source[index];
            if (char.IsDigit(character) && digitsToMask > 0)
            {
                destination[index] = '*';
                digitsToMask--;
            }
            else
            {
                destination[index] = character;
            }
        }

        return source.Length;
    }
}