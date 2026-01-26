using StockMind.Domain.Common;

namespace StockMind.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }

    private Address(string street, string number, string? complement, string city, string state, string zipCode, string country)
    {
        Street = street;
        Number = number;
        Complement = complement;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public static Address Create(string street, string number, string? complement, string city, string state, string zipCode, string country = "Brazil")
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Number cannot be empty", nameof(number));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(street.Trim(), number.Trim(), complement?.Trim(), city.Trim(), state.Trim(), zipCode.Trim(), country.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return Number;
        yield return Complement;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {Number}{(string.IsNullOrWhiteSpace(Complement) ? "" : $", {Complement}")}, {City}/{State}, {ZipCode}, {Country}";
}
