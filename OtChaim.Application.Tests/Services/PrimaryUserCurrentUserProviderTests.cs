using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using OtChaim.Application.Services;
using OtChaim.Domain.Users;

namespace OtChaim.Application.Tests.Services;

[TestFixture]
public class PrimaryUserCurrentUserProviderTests
{
    [Test]
    public async Task GetCurrentUserIdAsync_ShouldReturnIdFromPrimaryProfile()
    {
        // Arrange
        IUserRepository repository = Substitute.For<IUserRepository>();
        User expectedUser = new("Primary User", "primary@example.com", "123456789");
        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>(new List<User> { expectedUser }));

        UserProfileService profileService = new(repository);
        PrimaryUserCurrentUserProvider provider = new(profileService);
        CancellationTokenSource cts = new();

        // Act
        Guid result = await provider.GetCurrentUserIdAsync(cts.Token);

        // Assert
        result.Should().Be(expectedUser.Id);
        await repository.Received(1).GetAllAsync(cts.Token);
    }

    [Test]
    public async Task GetCurrentUserIdAsync_WhenNoUserExists_ShouldReturnDefaultUserId()
    {
        // Arrange
        IUserRepository repository = Substitute.For<IUserRepository>();
        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>()));

        UserProfileService profileService = new(repository);
        PrimaryUserCurrentUserProvider provider = new(profileService);

        // Act
        Guid result = await provider.GetCurrentUserIdAsync();

        // Assert
        result.Should().NotBe(Guid.Empty);
        await repository.Received(1).AddAsync(Arg.Is<User>(u => u.Id == result), Arg.Any<CancellationToken>());
    }
}
