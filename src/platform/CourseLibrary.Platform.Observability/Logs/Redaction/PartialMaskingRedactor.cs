using Microsoft.Extensions.Compliance.Redaction;

namespace CourseLibrary.Platform.Observability.Logs.Redaction;


public sealed class PartialMaskingRedactor : Redactor
{
    private readonly int _prefixLength;
    private readonly int _suffixLength;
    private readonly int _visibleLength;
    private readonly char _maskCharacter;

    public PartialMaskingRedactor(
        int prefixLength = 2,
        int suffixLength = 2,
        char maskCharacter = '*')
    {
        ArgumentOutOfRangeException.ThrowIfNegative(prefixLength);
        ArgumentOutOfRangeException.ThrowIfNegative(suffixLength);

        if (prefixLength == 0 && suffixLength == 0)
        {
            throw new ArgumentException("At least one character must be masked.");
        }

        _prefixLength = prefixLength;
        _suffixLength = suffixLength;
        _visibleLength = checked(prefixLength + suffixLength);
        _maskCharacter = maskCharacter;
    }


    public override int GetRedactedLength(
        ReadOnlySpan<char> input)
        => input.Length;


    public override int Redact(
        ReadOnlySpan<char> source,
        Span<char> destination)
    {
        if (source.Length <= _visibleLength)
        {
            destination[..source.Length].Fill(_maskCharacter);
            return source.Length;
        }


        var index = 0;


        source[.._prefixLength]
            .CopyTo(destination);

        index += _prefixLength;


        var maskLength =
            source.Length -
            _visibleLength;


        for (var i = 0; i < maskLength; i++)
        {
            destination[index++] = _maskCharacter;
        }


        source[^_suffixLength..]
            .CopyTo(destination[index..]);

        index += _suffixLength;


        return index;
    }
}