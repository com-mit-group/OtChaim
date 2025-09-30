using OtChaim.Domain.Common;

namespace OtChaim.Domain.Users;

/// <summary>
/// Encapsulates the personal profile details used to update a <see cref="User"/>.
/// </summary>
public sealed class PersonalProfileUpdate
{
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime? BirthDate { get; }
    public double? WeightInKg { get; }
    public string? BloodType { get; }
    public string? Address { get; }
    public Location? CurrentLocation { get; }
    public string? ProfilePicturePath { get; }

    public PersonalProfileUpdate(
        string firstName,
        string lastName,
        DateTime? birthDate,
        double? weightInKg,
        string? bloodType,
        string? address,
        Location? currentLocation,
        string? profilePicturePath)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        WeightInKg = weightInKg;
        BloodType = bloodType;
        Address = address;
        CurrentLocation = currentLocation;
        ProfilePicturePath = profilePicturePath;
    }
}
