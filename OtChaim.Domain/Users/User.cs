using OtChaim.Domain.Common;
using OtChaim.Domain.Notifications;
using OtChaim.Domain.Users.Events;
using System.Linq;

namespace OtChaim.Domain.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity
{
    /// <summary>
    /// Gets the structured representation of the user's name.
    /// </summary>
    public PersonName PersonName { get; private set; } = PersonName.Empty;
    /// <summary>
    /// Gets the user's combined name for backward compatibility (First + Last).
    /// </summary>
    public string Name => PersonName.Full;
    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName => PersonName.First;
    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName => PersonName.Last;
    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; private set; } = string.Empty;
    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    public string PhoneNumber { get; private set; } = string.Empty;
    /// <summary>
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; private set; }
    /// <summary>
    /// Gets the user's birth date if provided.
    /// </summary>
    public DateTime? BirthDate { get; private set; }
    /// <summary>
    /// Gets the user's weight in kilograms if provided.
    /// </summary>
    public double? WeightInKg { get; private set; }
    /// <summary>
    /// Gets the user's blood type.
    /// </summary>
    public string BloodType { get; private set; } = string.Empty;
    /// <summary>
    /// Gets the user's home address.
    /// </summary>
    public string Address { get; private set; } = string.Empty;
    /// <summary>
    /// Gets the user's current location.
    /// </summary>
    public Location CurrentLocation { get; private set; } = Location.Empty;
    /// <summary>
    /// Gets the path to the user's profile picture if one is set.
    /// </summary>
    public string ProfilePicturePath { get; private set; } = string.Empty;
    private readonly List<Guid> _subscriberIds = [];
    /// <summary>
    /// Gets the list of subscriber IDs.
    /// </summary>
    public IReadOnlyList<Guid> SubscriberIds => _subscriberIds.AsReadOnly();
    private readonly List<NotificationChannel> _notificationChannels = [];
    /// <summary>
    /// Gets the notification channels for the user.
    /// </summary>
    public IReadOnlyList<NotificationChannel> NotificationChannels => _notificationChannels.AsReadOnly();
    private readonly List<Subscription> _subscriptions = [];
    private bool _requireApproval = true;

    /// <summary>
    /// Gets the subscriptions for the user.
    /// </summary>
    public IReadOnlyList<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    /// <summary>
    /// Gets a user instance representing no user.
    /// </summary>
    public static User None { get; } = new User { Id = Guid.Empty };

    private User() { } // For EF Core

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User(string name, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));

        Id = Guid.NewGuid();
        SetNameFromFullName(name);
        Email = email;
        PhoneNumber = phoneNumber;
        IsActive = true;
    }

    /// <summary>
    /// Updates the core personal profile information for the user.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="birthDate">The optional birth date.</param>
    /// <param name="weightInKg">The optional weight in kilograms.</param>
    /// <param name="bloodType">The blood type.</param>
    /// <param name="address">The home address.</param>
    /// <param name="currentLocation">The current GPS location.</param>
    /// <param name="profilePicturePath">The path to the profile picture.</param>
    public void UpdatePersonalProfile(
        string firstName,
        string lastName,
        DateTime? birthDate,
        double? weightInKg,
        string bloodType,
        string address,
        Location? currentLocation,
        string? profilePicturePath)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        }

        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Birth date cannot be in the future", nameof(birthDate));
        }

        if (weightInKg.HasValue && weightInKg.Value <= 0)
        {
            throw new ArgumentException("Weight must be greater than zero", nameof(weightInKg));
        }

        UpdateNameFromParts(firstName, lastName);

        BirthDate = birthDate?.Date;
        WeightInKg = weightInKg;
        BloodType = (bloodType ?? string.Empty).Trim();
        Address = (address ?? string.Empty).Trim();
        CurrentLocation = (currentLocation ?? Location.Empty).Clone();
        ProfilePicturePath = profilePicturePath?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Updates the user's contact information.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="phoneNumber">The phone number.</param>
    public void UpdateContactInformation(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        }

        Email = email;
        PhoneNumber = phoneNumber;
    }

    /// <summary>
    /// Updates the extended profile information of the user.
    /// </summary>
    public void UpdateProfile(
        string? firstName,
        string? lastName,
        DateTime? birthday,
        double? weightKg,
        string? bloodType,
        string? address,
        Location? currentLocation,
        string? profilePicturePath,
        string? phone,
        string? email)
    {
        string? trimmedFirstName = firstName?.Trim();
        string? trimmedLastName = lastName?.Trim();

        if (!string.IsNullOrWhiteSpace(trimmedFirstName) || !string.IsNullOrWhiteSpace(trimmedLastName))
        {
            string newFirst = string.IsNullOrWhiteSpace(trimmedFirstName) ? PersonName.First : trimmedFirstName;
            string newLast = string.IsNullOrWhiteSpace(trimmedLastName) ? PersonName.Last : trimmedLastName;
            PersonName = new PersonName(newFirst, newLast);
        }

        if (birthday.HasValue && birthday.Value.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Birthday cannot be in the future", nameof(birthday));
        BirthDate = birthday?.Date;

        if (weightKg.HasValue && weightKg.Value <= 0)
            throw new ArgumentException("Weight must be positive", nameof(weightKg));
        WeightInKg = weightKg;

        if (!string.IsNullOrWhiteSpace(bloodType)) BloodType = bloodType.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(address)) Address = address.Trim();
        if (currentLocation is not null)
        {
            CurrentLocation = currentLocation.Clone();
        }
        if (!string.IsNullOrWhiteSpace(profilePicturePath)) ProfilePicturePath = profilePicturePath.Trim();

        if (!string.IsNullOrWhiteSpace(phone)) PhoneNumber = phone.Trim();
        if (!string.IsNullOrWhiteSpace(email)) Email = email.Trim();
    }

    /// <summary>
    /// Adds a subscriber to the user.
    /// </summary>
    public void AddSubscriber(Guid subscriberId)
    {
        if (!_subscriberIds.Contains(subscriberId))
        {
            _subscriberIds.Add(subscriberId);
        }
    }

    /// <summary>
    /// Removes a subscriber from the user.
    /// </summary>
    public void RemoveSubscriber(Guid subscriberId)
    {
        _subscriberIds.Remove(subscriberId);
    }

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Toggles the subscription approval requirement.
    /// </summary>
    public void ToggleApproval()
    {
        _requireApproval = !_requireApproval;
    }

    /// <summary>
    /// Returns whether the user requires subscription approval.
    /// </summary>
    public bool RequiresSubscriptionApproval() => _requireApproval;

    /// <summary>
    /// Handles a subscription requested event.
    /// </summary>
    public void OnSubscriptionRequested(SubscriptionRequested subscriptionEvent)
    {
        Subscription subscription = new(subscriptionEvent.SubscriberId, subscriptionEvent.SubscribedToId, RequiresSubscriptionApproval());
        _subscriptions.Add(subscription);
    }

    /// <summary>
    /// Handles a subscription approved event.
    /// </summary>
    public void OnSubscriptionApproved(SubscriptionApproved subscriptionEvent)
    {
        Subscription? subscription = _subscriptions.FirstOrDefault(s => s.SubscriberId == subscriptionEvent.SubscriberId && s.SubscribedToId == subscriptionEvent.SubscribedToId);
        subscription?.Approve();
    }

    /// <summary>
    /// Handles a subscription rejected event.
    /// </summary>
    public void OnSubscriptionRejected(SubscriptionRejected subscriptionEvent)
    {
        Subscription? subscription = _subscriptions.FirstOrDefault(s => s.SubscriberId == subscriptionEvent.SubscriberId && s.SubscribedToId == subscriptionEvent.SubscribedToId);
        subscription?.Reject();
    }

    private void SetNameFromFullName(string name)
    {
        PersonName = PersonName.FromFullName(name);
    }

    private void UpdateNameFromParts(string firstName, string lastName)
    {
        PersonName = new PersonName(firstName, lastName);
    }
}
