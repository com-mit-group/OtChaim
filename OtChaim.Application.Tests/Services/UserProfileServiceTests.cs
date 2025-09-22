using FluentAssertions;
using NSubstitute;
using OtChaim.Application.Services;
using OtChaim.Domain.Users;

namespace OtChaim.Application.Tests.Services;

[TestFixture]
public class UserProfileServiceTests
{
    private IUserRepository _userRepository = null!;
    private UserProfileService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _service = new UserProfileService(_userRepository);
    }

    [Test]
    public async Task GetOrCreatePrimaryUserAsync_ShouldReturnExistingUser()
    {
        // Arrange
        User existing = new User("Existing User", "existing@example.com", "123");
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User> { existing });

        // Act
        User result = await _service.GetOrCreatePrimaryUserAsync();

        // Assert
        result.Should().Be(existing);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetOrCreatePrimaryUserAsync_ShouldCreateDefaultUser_WhenNoneExists()
    {
        // Arrange
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(new List<User>());

        // Act
        User result = await _service.GetOrCreatePrimaryUserAsync();

        // Assert
        await _userRepository.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        result.FirstName.Should().Be("Finn");
        result.LastName.Should().Be("Mond");
        result.Email.Should().Be("Finn@moon.com");
    }

    [Test]
    public async Task SaveAsync_ShouldPersistUsingRepository()
    {
        // Arrange
        User user = new User("John Doe", "john@example.com", "123");

        // Act
        await _service.SaveAsync(user);

        // Assert
        await _userRepository.Received(1).SaveAsync(user, Arg.Any<CancellationToken>());
    }
}
