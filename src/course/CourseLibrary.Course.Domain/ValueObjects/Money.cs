using CourseLibrary.Core.Domain.Abstractions.ValueObjects;
using System.Globalization;

namespace CourseLibrary.Course.Domain.ValueObjects;

public readonly record struct Money(decimal Value)
    : IStronglyTypedValue<decimal>
{
    public static Money Create(
        decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException(
                "Amount cannot be negative.",
                nameof(amount));
        }

        return new Money(amount);
    }

    public static implicit operator Money(decimal amount)
    {
        return Create(amount);
    }

    public static implicit operator decimal(Money money)
    {
        return money.Value;
    }
    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}