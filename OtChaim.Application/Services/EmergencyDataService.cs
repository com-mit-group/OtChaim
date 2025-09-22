using System;
using System.Collections.ObjectModel;
using OtChaim.Domain.EmergencyEvents;
using OtChaim.Domain.Users;

namespace OtChaim.Application.Services;

/// <summary>
/// Provides data operations for emergencies and users in the OtChaim application.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmergencyDataService"/> class.
/// </remarks>
/// <param name="emergencyRepository">The emergency repository.</param>
/// <param name="userRepository">The user repository.</param>
public class EmergencyDataService(IEmergencyRepository emergencyRepository, IUserRepository userRepository)
{
    private readonly IEmergencyRepository _emergencyRepository = emergencyRepository;
    private readonly IUserRepository _userRepository = userRepository;

    /// <summary>
    /// Loads active emergencies into the provided collection.
    /// </summary>
    /// <param name="emergencies">The collection to populate with active emergencies.</param>
    public async Task LoadActiveEmergenciesAsync(ObservableCollection<Emergency> emergencies)
    {
        if (emergencies is null)
        {
            return;
        }

        try
        {
            IReadOnlyList<Emergency> allEmergencies =
                await _emergencyRepository.GetActiveAsync() ?? Array.Empty<Emergency>();
            emergencies.Clear();
            foreach (Emergency emergency in allEmergencies)
            {
                emergencies.Add(emergency);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading emergencies: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads all users into the provided collection.
    /// </summary>
    /// <param name="users">The collection to populate with users.</param>
    public async Task LoadUsersAsync(ObservableCollection<User> users)
    {
        if (users is null)
        {
            return;
        }

        try
        {
            IReadOnlyList<User> allUsers =
                await _userRepository.GetAllAsync() ?? Array.Empty<User>();
            users.Clear();
            foreach (User user in allUsers)
            {
                users.Add(user);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading users: {ex.Message}");
        }
    }
}
