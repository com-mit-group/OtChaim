using OtChaim.Domain.Common;

namespace OtChaim.Domain.Users;

/// <summary>
/// Encapsulates the personal profile details used to update a <see cref="User"/>.
/// </summary>
public sealed record PersonalProfileUpdate
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public DateTime? BirthDate { get; init; }

    public double? WeightInKg { get; init; }

    public string? BloodType { get; init; }

    public string? Address { get; init; }

    public Location? CurrentLocation { get; init; }

    public string? ProfilePicturePath { get; init; }
}
