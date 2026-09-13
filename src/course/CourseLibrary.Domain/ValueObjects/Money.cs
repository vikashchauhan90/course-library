using System.Globalization;

namespace CourseLibrary.Domain.ValueObjects;

public readonly record struct Money(decimal Amount)
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

    public static explicit operator Money(decimal amount)
    {
        return Create(amount);
    }

    public static explicit operator decimal(Money money)
    {
        return money.Amount;
    }
    public override string ToString()
    {
        return Amount.ToString(CultureInfo.InvariantCulture);
    }
}