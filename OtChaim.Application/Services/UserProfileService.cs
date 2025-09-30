using OtChaim.Domain.Common;
using OtChaim.Domain.Users;
using System.Linq;

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
        User? existing = users.FirstOrDefault();
        if (existing is not null)
        {
            return existing;
        }

        User defaultUser = new User("Finn Mond", "Finn@moon.com", "+71/182637263");
        defaultUser.UpdatePersonalProfile(new PersonalProfileUpdate(
            "Finn",
            "Mond",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            83,
            "A",
            "Mondstrasse 3, 71626 Bonn",
            Location.Empty,
            string.Empty));

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
