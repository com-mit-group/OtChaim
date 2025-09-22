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
        user.UpdatePersonalProfile("John", "Doe", birthDate, 82.3, "O-", "123 Main St", location, "profile.jpg");

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Name.Should().Be("John Doe");
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

        Action act = () => user.UpdatePersonalProfile(string.Empty, "Doe", DateTime.UtcNow, 70, "A+", "", Location.Empty, null);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void UpdatePersonalProfile_WithFutureBirthDate_ShouldThrow()
    {
        User user = new User("Jane Doe", "jane@example.com", "123456789");
        DateTime futureBirthDate = DateTime.Today.AddDays(1);

        Action act = () => user.UpdatePersonalProfile("Jane", "Doe", futureBirthDate, 70, "A+", "", Location.Empty, null);

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
}
