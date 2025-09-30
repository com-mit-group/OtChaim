using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using OtChaim.Application.Services;
using OtChaim.Domain.Common;
using OtChaim.Domain.Users;
using Location = OtChaim.Domain.Common.Location;


#if !UNIT_TESTS
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
#endif

namespace OtChaim.Presentation.MAUI.ViewModels.Settings;

/// <summary>
/// View model backing the settings tab user information page.
/// </summary>
public partial class UserInfoViewModel : ObservableObject
{
    private readonly UserProfileService _userProfileService;
    private User? _user;
    private Location _activeLocation = Location.Empty;
    private bool _isInitialized;
    private DateTime? _initialBirthDate;
    private double? _initialWeight;
    private string _initialFirstName = string.Empty;
    private string _initialLastName = string.Empty;
    private string _initialBloodType = string.Empty;
    private string _initialAddress = string.Empty;
    private Location _initialLocation = Location.Empty;
    private string _initialProfilePicturePath = string.Empty;

    /// <summary>
    /// Available blood type options.
    /// </summary>
    public ObservableCollection<string> BloodTypes { get; } = new(
    [
        "A",
        "A+",
        "A-",
        "B",
        "B+",
        "B-",
        "AB",
        "AB+",
        "AB-",
        "O",
        "O+",
        "O-"
    ]);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProfileImage))]
    [NotifyPropertyChangedFor(nameof(HasProfilePicture))]
    private string _profilePicturePath = string.Empty;

    /// <summary>
    /// Gets the image source for the profile picture.
    /// </summary>
    public ImageSource ProfileImage => string.IsNullOrWhiteSpace(ProfilePicturePath)
        ? ImageSource.FromFile("profile_picture.png")
        : ImageSource.FromFile(ProfilePicturePath);

    /// <summary>
    /// Gets a value indicating whether a profile picture has been chosen.
    /// </summary>
    public bool HasProfilePicture => !string.IsNullOrWhiteSpace(ProfilePicturePath);

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private DateTime _birthday = DateTime.Today;

    [ObservableProperty]
    private string _weight = string.Empty;

    [ObservableProperty]
    private string _selectedBloodType = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _currentLocation = "Get GPS coordinates";

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
    [NotifyPropertyChangedFor(nameof(StatusMessageColor))]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusMessageColor))]
    private bool _isError;

    /// <summary>
    /// Gets a value indicating whether there is a status message to display.
    /// </summary>
    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    /// <summary>
    /// Gets the color for the status message.
    /// </summary>
    public Color StatusMessageColor => IsError ? Colors.Red : Colors.Green;

    public UserInfoViewModel(UserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Initializes the view model by loading the current profile.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (_user is null)
        {
            SetStatus("No user profile loaded.", true);
            return;
        }

        if (!TryValidate(out double? weightValue, out string normalizedBloodType))
        {
            return;
        }

        try
        {
            IsBusy = true;
            DateTime selectedBirthday = DateTime.SpecifyKind(Birthday.Date, DateTimeKind.Utc);

            SelectedBloodType = EnsureBloodType(normalizedBloodType);

            PersonalProfileUpdate profile = new(
                FirstName,
                LastName,
                selectedBirthday,
                weightValue,
                normalizedBloodType,
                Address,
                _activeLocation,
                ProfilePicturePath);

            _user.UpdatePersonalProfile(profile);

            await _userProfileService.SaveAsync(_user);

            CacheSnapshot();
            SetStatus("Profile saved successfully.", false);
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to save profile: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ResetProfile()
    {
        if (_user is null)
        {
            return;
        }

        ProfilePicturePath = _initialProfilePicturePath;
        FirstName = _initialFirstName;
        LastName = _initialLastName;
        Birthday = (_initialBirthDate ?? DateTime.Today).Date;
        Weight = FormatWeight(_initialWeight);
        SelectedBloodType = EnsureBloodType(_initialBloodType);
        Address = _initialAddress;
        _activeLocation = _initialLocation.Clone();
        CurrentLocation = FormatLocation(_activeLocation);
        Phone = _user.PhoneNumber;
        Email = _user.Email;
        ClearStatus();
    }

    [RelayCommand]
#if UNIT_TESTS
    private Task ChooseProfilePictureAsync() => Task.CompletedTask;
#else
    private async Task ChooseProfilePictureAsync()
    {
        try
        {
            FileResult? photo = await MediaPicker.PickPhotoAsync();
            if (photo is not null)
            {
                ProfilePicturePath = photo.FullPath;
                SetStatus("Profile picture updated.", false);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Unable to select photo: {ex.Message}", true);
        }

        return;
    }
#endif

    [RelayCommand]
    private void RemoveProfilePicture()
    {
        ProfilePicturePath = string.Empty;
    }

    [RelayCommand]
#if UNIT_TESTS
    private Task RefreshLocationAsync() => Task.CompletedTask;
#else
    private async Task RefreshLocationAsync()
    {
        try
        {
            IsBusy = true;
            GeolocationRequest request = new(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            Microsoft.Maui.Devices.Sensors.Location? location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                SetStatus("Unable to determine location.", true);
                return;
            }

            _activeLocation = new Location(location.Latitude, location.Longitude, Address);
            CurrentLocation = FormatLocation(_activeLocation);
            SetStatus("Location updated.", false);
        }
        catch (FeatureNotSupportedException ex)
        {
            SetStatus($"GPS not supported: {ex.Message}", true);
        }
        catch (FeatureNotEnabledException ex)
        {
            SetStatus($"GPS is disabled: {ex.Message}", true);
        }
        catch (PermissionException ex)
        {
            SetStatus($"GPS permission denied: {ex.Message}", true);
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to retrieve location: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }
#endif

    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            _user = await _userProfileService.GetOrCreatePrimaryUserAsync();
            ApplyUser(_user);
            ClearStatus();
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load profile: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyUser(User user)
    {
        _initialFirstName = string.IsNullOrWhiteSpace(user.FirstName) ? user.Name : user.FirstName;
        _initialLastName = user.LastName;
        _initialBirthDate = user.BirthDate;
        _initialWeight = user.WeightInKg;
        _initialBloodType = user.BloodType;
        _initialAddress = user.Address;
        _initialLocation = user.CurrentLocation.Clone();
        _initialProfilePicturePath = user.ProfilePicturePath;

        ProfilePicturePath = _initialProfilePicturePath;
        FirstName = _initialFirstName;
        LastName = _initialLastName;
        Birthday = (_initialBirthDate ?? DateTime.Today).Date;
        Weight = FormatWeight(_initialWeight);
        SelectedBloodType = EnsureBloodType(_initialBloodType);
        Address = _initialAddress;
        _activeLocation = _initialLocation.Clone();
        CurrentLocation = FormatLocation(_activeLocation);
        Phone = user.PhoneNumber;
        Email = user.Email;
    }

    private void CacheSnapshot()
    {
        if (_user is null)
        {
            return;
        }

        _initialFirstName = _user.FirstName;
        _initialLastName = _user.LastName;
        _initialBirthDate = _user.BirthDate;
        _initialWeight = _user.WeightInKg;
        _initialBloodType = _user.BloodType;
        _initialAddress = _user.Address;
        _initialLocation = _user.CurrentLocation.Clone();
        _initialProfilePicturePath = _user.ProfilePicturePath;
    }

    private bool TryValidate(out double? weightValue, out string normalizedBloodType)
    {
        List<string> errors = [];
        weightValue = null;
        normalizedBloodType = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            errors.Add("First name is required.");
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            errors.Add("Last name is required.");
        }

        if (Birthday.Date > DateTime.Today)
        {
            errors.Add("Birthday cannot be in the future.");
        }

        if (!string.IsNullOrWhiteSpace(Weight))
        {
            if (!double.TryParse(Weight, NumberStyles.Float, CultureInfo.CurrentCulture, out double parsedWeight) || parsedWeight <= 0)
            {
                errors.Add("Weight must be a positive number.");
            }
            else
            {
                weightValue = parsedWeight;
            }
        }

        string? trimmedBloodType = SelectedBloodType?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedBloodType))
        {
            errors.Add("Blood type must be selected.");
        }
        else
        {
            normalizedBloodType = trimmedBloodType.ToUpperInvariant();
        }

        if (errors.Count > 0)
        {
            SetStatus(string.Join(Environment.NewLine, errors), true);
            return false;
        }

        return true;
    }

    private static string FormatWeight(double? weight)
        => weight.HasValue
            ? weight.Value.ToString("0.##", CultureInfo.CurrentCulture)
            : string.Empty;

    private string EnsureBloodType(string value)
    {
        string normalized = string.IsNullOrWhiteSpace(value)
            ? BloodTypes.First()
            : value.Trim().ToUpperInvariant();

        if (!BloodTypes.Contains(normalized))
        {
            BloodTypes.Add(normalized);
        }

        return normalized;
    }

    private static string FormatLocation(Location location)
    {
        bool hasLocation = !(Math.Abs(location.Latitude + 1) < 0.000001 && Math.Abs(location.Longitude + 1) < 0.000001)
            && location.Latitude is >= -90 and <= 90
            && location.Longitude is >= -180 and <= 180;

        return hasLocation
            ? string.Create(CultureInfo.InvariantCulture, $"{location.Latitude:F5}, {location.Longitude:F5}")
            : "Get GPS coordinates";
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsError = isError;
    }

    private void ClearStatus()
    {
        StatusMessage = string.Empty;
        IsError = false;
    }
}
