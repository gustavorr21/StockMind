using StockMind.Domain.Common;
using System.Text.RegularExpressions;

namespace StockMind.Domain.ValueObjects;

public sealed class Phone : ValueObject
{
    public string Number { get; private set; }

    private Phone(string number)
    {
        Number = number;
    }

    public static Phone Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        var cleaned = Regex.Replace(phoneNumber, @"[^\d]", "");

        if (cleaned.Length < 10 || cleaned.Length > 13)
            throw new ArgumentException("Invalid phone number format", nameof(phoneNumber));

        return new Phone(cleaned);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Number;
    }

    public override string ToString() => Number;

    public static implicit operator string(Phone phone) => phone.Number;
}
