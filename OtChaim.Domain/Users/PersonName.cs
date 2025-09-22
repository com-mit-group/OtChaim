using OtChaim.Domain.Common;

namespace OtChaim.Domain.Users;

/// <summary>
/// Value object representing a person's name (first + last) with basic normalization and formatting.
/// </summary>
public sealed class PersonName : ValueObject
{
    public string First { get; private set; }
    public string Last { get; private set; }
    public string Full => string.Join(' ', new[] { First, Last }.Where(p => !string.IsNullOrWhiteSpace(p)));

    public static PersonName Empty { get; } = new(string.Empty, string.Empty);

    private PersonName() { First = Last = string.Empty; }

    public PersonName(string first, string last)
    {
        First = (first ?? string.Empty).Trim();
        Last = (last ?? string.Empty).Trim();
    }

    /// <summary>
    /// Creates a <see cref="PersonName"/> from a single full name string.
    /// </summary>
    /// <param name="fullName">The full name to split into first and last parts.</param>
    public static PersonName FromFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Empty;
        }

        string trimmed = fullName.Trim();
        string[] parts = trimmed.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string first = parts.Length > 0 ? parts[0] : string.Empty;
        string last = parts.Length > 1 ? parts[1] : string.Empty;
        return new PersonName(first, last);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return First.ToUpperInvariant();
        yield return Last.ToUpperInvariant();
    }

    public override string ToString() => Full;
}
