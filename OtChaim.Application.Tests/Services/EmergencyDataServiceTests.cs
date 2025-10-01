using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using OtChaim.Application.Services;
using OtChaim.Domain.EmergencyEvents;
using OtChaim.Domain.Users;

namespace OtChaim.Application.Tests.Services;

[TestFixture]
public class EmergencyDataServiceTests
{
    private IEmergencyRepository _emergencyRepository = null!;
    private IUserRepository _userRepository = null!;
    private EmergencyDataService _service = null!;

    [SetUp]
    public void Setup()
    {
        _emergencyRepository = Substitute.For<IEmergencyRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _service = new EmergencyDataService(_emergencyRepository, _userRepository);
    }

    [Test]
    public void Constructor_ShouldInitializeService()
    {
        _service.Should().NotBeNull();
    }

    [Test]
    public async Task LoadActiveEmergenciesAsync_WhenRepositoryReturnsEmptyCollection_ShouldLeaveCollectionEmpty()
    {
        var emergencies = new ObservableCollection<Emergency>();

        _emergencyRepository.GetActiveAsync()
            .Returns(Task.FromResult<IReadOnlyList<Emergency>>(Array.Empty<Emergency>()));

        await _service.LoadActiveEmergenciesAsync(emergencies);

        emergencies.Should().BeEmpty();
    }

    [Test]
    public async Task LoadUsersAsync_WhenRepositoryReturnsEmptyCollection_ShouldLeaveCollectionEmpty()
    {
        var users = new ObservableCollection<User>();

        _userRepository.GetAllAsync()
            .Returns(Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>()));

        await _service.LoadUsersAsync(users);

        users.Should().BeEmpty();
    }

    [Test]
    public async Task LoadActiveEmergenciesAsync_WhenRepositoryReturnsNull_ShouldLeaveCollectionEmpty()
    {
        var emergencies = new ObservableCollection<Emergency>();

        _emergencyRepository.GetActiveAsync()
            .Returns(Task.FromResult<IReadOnlyList<Emergency>>(null!));

        Func<Task> action = () => _service.LoadActiveEmergenciesAsync(emergencies);

        await action.Should().NotThrowAsync();
        emergencies.Should().BeEmpty();
    }

    [Test]
    public async Task LoadUsersAsync_WhenRepositoryReturnsNull_ShouldLeaveCollectionEmpty()
    {
        var users = new ObservableCollection<User>();

        _userRepository.GetAllAsync()
            .Returns(Task.FromResult<IReadOnlyList<User>>(null!));

        Func<Task> action = () => _service.LoadUsersAsync(users);

        await action.Should().NotThrowAsync();
        users.Should().BeEmpty();
    }

    [Test]
    public async Task LoadActiveEmergenciesAsync_WithNullCollection_ShouldReturnCleanly()
    {
        Func<Task> action = () => _service.LoadActiveEmergenciesAsync(null!);

        await action.Should().NotThrowAsync();
        _emergencyRepository.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public async Task LoadUsersAsync_WithNullCollection_ShouldReturnCleanly()
    {
        Func<Task> action = () => _service.LoadUsersAsync(null!);

        await action.Should().NotThrowAsync();
        _userRepository.ReceivedCalls().Should().BeEmpty();
    }
}
