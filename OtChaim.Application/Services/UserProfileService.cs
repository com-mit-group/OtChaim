using OtChaim.Domain.Common;
using OtChaim.Domain.Users;

namespace OtChaim.Application.Services;

/// <summary>
/// Provides access to the user's personal profile information.
/// </summary>
public class UserProfileService(IUserRepository userRepository)
{
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Retrieves the primary user profile, creating a default profile when none exists.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<User> GetOrCreatePrimaryUserAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> users = await _userRepository.GetAllAsync(cancellationToken) ?? Array.Empty<User>();
        User? existing = users.Count > 0 ? users[0] : null;
        if (existing is not null)
        {
            return existing;
        }

        User defaultUser = new User("Finn Mond", "Finn@moon.com", "+71/182637263");
        defaultUser.UpdatePersonalProfile(new PersonalProfileUpdate
        {
            FirstName = "Finn",
            LastName = "Mond",
            BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            WeightInKg = 83,
            BloodType = "A",
            Address = "Mondstrasse 3, 71626 Bonn",
            CurrentLocation = Location.Empty,
            ProfilePicturePath = string.Empty,
        });

        await _userRepository.AddAsync(defaultUser, cancellationToken);
        return defaultUser;
    }

    /// <summary>
    /// Persists the provided user profile changes.
    /// </summary>
    /// <param name="user">The user to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task SaveAsync(User user, CancellationToken cancellationToken = default)
        => _userRepository.SaveAsync(user, cancellationToken);
}
