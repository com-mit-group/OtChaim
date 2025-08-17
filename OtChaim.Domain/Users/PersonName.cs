using OtChaim.Domain.Common;

namespace OtChaim.Domain.Users;

/// <summary>
/// Value object representing a person's name (first + last) with basic normalization and formatting.
/// </summary>
public sealed class PersonName : ValueObject
{
    public string First { get; }
    public string Last { get; }
    public string Full => string.Join(' ', new[] { First, Last }.Where(p => !string.IsNullOrWhiteSpace(p)));

    public static PersonName Empty { get; } = new(string.Empty, string.Empty);

    private PersonName() { First = Last = string.Empty; }

    public PersonName(string first, string last)
    {
        First = (first ?? string.Empty).Trim();
        Last = (last ?? string.Empty).Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return First.ToUpperInvariant();
        yield return Last.ToUpperInvariant();
    }

    public override string ToString() => Full;
}
