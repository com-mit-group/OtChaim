using FluentAssertions;
using OtChaim.Domain.Common;
using OtChaim.Domain.Users;

namespace OtChaim.Domain.Tests.Users;

[TestFixture]
public class UserTests
{
    [Test]
    public void UpdatePersonalProfile_WithValidValues_ShouldUpdateUserState()
    {
        // Arrange
        User user = new User("John Doe", "john@example.com", "123456789");
        DateTime birthDate = new DateTime(1990, 5, 12, 0, 0, 0, DateTimeKind.Utc);
        Location location = new Location(45.1234, -93.1234, "Home");

        // Act
        PersonalProfileUpdate profile = new()
        {
            FirstName = "John",
            LastName = "Doe",
            BirthDate = birthDate,
            WeightInKg = 82.3,
            BloodType = "O-",
            Address = "123 Main St",
            CurrentLocation = location,
            ProfilePicturePath = "profile.jpg",
        };

        user.UpdatePersonalProfile(profile);

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Name.Should().Be("John Doe");
        user.PersonName.Full.Should().Be("John Doe");
        user.BirthDate.Should().Be(birthDate.Date);
        user.WeightInKg.Should().Be(82.3);
        user.BloodType.Should().Be("O-");
        user.Address.Should().Be("123 Main St");
        user.CurrentLocation.Latitude.Should().Be(location.Latitude);
        user.CurrentLocation.Longitude.Should().Be(location.Longitude);
        user.ProfilePicturePath.Should().Be("profile.jpg");
    }

    [Test]
    public void UpdatePersonalProfile_WithEmptyFirstName_ShouldThrow()
    {
        User user = new User("Jane Doe", "jane@example.com", "123456789");

        PersonalProfileUpdate profile = new()
        {
            FirstName = string.Empty,
            LastName = "Doe",
            BirthDate = DateTime.UtcNow,
            WeightInKg = 70,
            BloodType = "A+",
            Address = string.Empty,
            CurrentLocation = Location.Empty,
            ProfilePicturePath = null,
        };

        Action act = () => user.UpdatePersonalProfile(profile);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void UpdatePersonalProfile_WithFutureBirthDate_ShouldThrow()
    {
        User user = new User("Jane Doe", "jane@example.com", "123456789");
        DateTime futureBirthDate = DateTime.Today.AddDays(1);

        PersonalProfileUpdate profile = new()
        {
            FirstName = "Jane",
            LastName = "Doe",
            BirthDate = futureBirthDate,
            WeightInKg = 70,
            BloodType = "A+",
            Address = string.Empty,
            CurrentLocation = Location.Empty,
            ProfilePicturePath = null,
        };

        Action act = () => user.UpdatePersonalProfile(profile);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void UpdateContactInformation_ShouldUpdateEmailAndPhone()
    {
        User user = new User("Jane Doe", "jane@example.com", "123456789");

        user.UpdateContactInformation("updated@example.com", "987654321");

        user.Email.Should().Be("updated@example.com");
        user.PhoneNumber.Should().Be("987654321");
    }

    [Test]
    public void Constructor_ShouldNormalizePersonName()
    {
        User user = new User("  Alice   Cooper ", "alice@example.com", "123456789");

        user.PersonName.First.Should().Be("Alice");
        user.PersonName.Last.Should().Be("Cooper");
        user.Name.Should().Be("Alice Cooper");
    }

    [Test]
    public void UpdateProfile_WithPartialName_ShouldPreserveExistingParts()
    {
        User user = new User("Jane Doe", "jane@example.com", "123456789");

        user.UpdateProfile(null, "Smith", null, null, null, null, null, null, null, null);

        user.PersonName.Full.Should().Be("Jane Smith");
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
    }
}
