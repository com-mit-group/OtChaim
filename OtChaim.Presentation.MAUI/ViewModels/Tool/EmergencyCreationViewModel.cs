using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtChaim.Application.Common;
using OtChaim.Application.EmergencyEvents.Commands;
using OtChaim.Application.ViewModels;
using OtChaim.Domain.Common;
using OtChaim.Domain.EmergencyEvents;

namespace OtChaim.Presentation.MAUI.ViewModels.Tool;

/// <summary>
/// ViewModel for managing the unified emergency creation popup interface.
/// This ViewModel provides a comprehensive interface for creating new emergencies with
/// all available options including emergency type, location, attachments, contact preferences,
/// and custom messages.
/// </summary>
/// <remarks>
/// The EmergencyCreationViewModel consolidates all emergency creation functionality into
/// a single, user-friendly interface. It handles file attachments, location services,
/// contact method preferences, and provides real-time validation and feedback to users.
/// </remarks>
public partial class EmergencyCreationViewModel : BaseEmergencyViewModel
{
    private readonly ICommandHandler<StartEmergency> _startEmergencyHandler;

    /// <summary>
    /// Event raised when an emergency is successfully created.
    /// </summary>
    /// <remarks>
    /// This event is triggered after the emergency has been successfully processed and
    /// the StartEmergency command has been executed. It signals to the parent ViewModel
    /// that the popup should be closed and the emergency list refreshed.
    /// </remarks>
    public event EventHandler? EmergencyCreated;

    /// <summary>
    /// Event raised when the emergency creation is cancelled by the user.
    /// </summary>
    /// <remarks>
    /// This event is triggered when the user explicitly cancels the emergency creation
    /// process, allowing the parent ViewModel to close the popup without any action.
    /// </remarks>
    public event EventHandler? Cancelled;

    /// <summary>
    /// Gets or sets the currently selected emergency type.
    /// </summary>
    /// <remarks>
    /// This property determines the type of emergency being created and may affect
    /// the default message template and other UI elements.
    /// </remarks>
    [ObservableProperty]
    private EmergencyType _selectedEmergencyType;

    /// <summary>
    /// Gets or sets the custom emergency message.
    /// </summary>
    /// <remarks>
    /// This property allows users to provide a custom message describing the emergency
    /// situation. If left empty, a default message based on the selected emergency type
    /// will be used.
    /// </remarks>
    [ObservableProperty]
    private string _emergencyMessage = string.Empty;

    /// <summary>
    /// Gets or sets the location description for the emergency.
    /// </summary>
    /// <remarks>
    /// This property provides a human-readable description of the emergency location,
    /// such as "Downtown Office Building" or "Central Park".
    /// </remarks>
    [ObservableProperty]
    private string _locationDescription = string.Empty;

    /// <summary>
    /// Gets or sets the latitude coordinate for the emergency location.
    /// </summary>
    /// <remarks>
    /// This property stores the geographical latitude of the emergency location.
    /// It is used along with longitude to provide precise location information.
    /// </remarks>
    [ObservableProperty]
    private double _latitude;

    /// <summary>
    /// Gets or sets the longitude coordinate for the emergency location.
    /// </summary>
    /// <remarks>
    /// This property stores the geographical longitude of the emergency location.
    /// It is used along with latitude to provide precise location information.
    /// </remarks>
    [ObservableProperty]
    private double _longitude;

    /// <summary>
    /// Gets or sets a value indicating whether the emergency should be sent via email.
    /// </summary>
    /// <remarks>
    /// When true, the emergency notification will be sent to contacts via email.
    /// This preference is stored in the emergency attachments for processing.
    /// </remarks>
    [ObservableProperty]
    private bool _sendEmail = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emergency should be sent via SMS.
    /// </summary>
    /// <remarks>
    /// When true, the emergency notification will be sent to contacts via SMS.
    /// This preference is stored in the emergency attachments for processing.
    /// </remarks>
    [ObservableProperty]
    private bool _sendSms = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emergency should be sent via messenger.
    /// </summary>
    /// <remarks>
    /// When true, the emergency notification will be sent to contacts via messenger.
    /// This preference is stored in the emergency attachments for processing.
    /// </remarks>
    [ObservableProperty]
    private bool _sendMessenger = false;

    /// <summary>
    /// Gets or sets a value indicating whether personal information should be attached.
    /// </summary>
    /// <remarks>
    /// When true, the user's personal information will be included as an attachment
    /// to the emergency notification. This can be toggled on/off by the user.
    /// </remarks>
    [ObservableProperty]
    private bool _attachPersonalInfo = true;

    /// <summary>
    /// Gets or sets a value indicating whether medical information should be attached.
    /// </summary>
    /// <remarks>
    /// When true, the user's medical information will be included as an attachment
    /// to the emergency notification. This can be toggled on/off by the user.
    /// </remarks>
    [ObservableProperty]
    private bool _attachMedicalInfo = true;

    /// <summary>
    /// Gets or sets a value indicating whether GPS location should be attached.
    /// </summary>
    /// <remarks>
    /// When true, the current GPS coordinates will be included as an attachment
    /// to the emergency notification. This can be toggled on/off by the user.
    /// </remarks>
    [ObservableProperty]
    private bool _attachGps = true;

    /// <summary>
    /// Gets or sets the path to the attached picture file.
    /// </summary>
    /// <remarks>
    /// This property stores the file path of a picture that has been selected
    /// by the user to be attached to the emergency notification.
    /// </remarks>
    [ObservableProperty]
    private string _attachedPicturePath = string.Empty;

    /// <summary>
    /// Gets or sets the path to the attached document file.
    /// </summary>
    /// <remarks>
    /// This property stores the file path of a document that has been selected
    /// by the user to be attached to the emergency notification.
    /// </remarks>
    [ObservableProperty]
    private string _attachedDocumentPath = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether group contacts are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isGroupSelected = true;

    /// <summary>
    /// Gets or sets a value indicating whether single contacts are selected.
    /// </summary>
    [ObservableProperty]
    private bool _isSingleSelected = false;

    /// <summary>
    /// Gets or sets a value indicating whether email is selected as a contact method.
    /// </summary>
    [ObservableProperty]
    private bool _isEmailSelected = true;

    /// <summary>
    /// Gets or sets a value indicating whether SMS is selected as a contact method.
    /// </summary>
    [ObservableProperty]
    private bool _isSmsSelected = true;

    /// <summary>
    /// Gets or sets a value indicating whether messenger is selected as a contact method.
    /// </summary>
    [ObservableProperty]
    private bool _isMessengerSelected = false;

    /// <summary>
    /// Gets a value indicating whether a picture is attached.
    /// </summary>
    public bool IsPictureAttached => !string.IsNullOrEmpty(AttachedPicturePath);

    /// <summary>
    /// Gets a value indicating whether personal info is attached.
    /// </summary>
    public bool IsPersonalInfoAttached => AttachPersonalInfo;

    /// <summary>
    /// Gets a value indicating whether medical info is attached.
    /// </summary>
    public bool IsMedicalInfoAttached => AttachMedicalInfo;

    /// <summary>
    /// Gets a value indicating whether GPS is attached.
    /// </summary>
    public bool IsGpsAttached => AttachGps;

    /// <summary>
    /// Gets a value indicating whether a document is attached.
    /// </summary>
    public bool IsDocumentAttached => !string.IsNullOrEmpty(AttachedDocumentPath);

    /// <summary>
    /// Initializes a new instance of the <see cref="EmergencyCreationViewModel"/> class.
    /// </summary>
    /// <param name="startEmergencyHandler">The command handler for starting emergencies.</param>
    /// <remarks>
    /// The constructor initializes the ViewModel with default values and sets up
    /// the emergency type to the first available option.
    /// </remarks>
    public EmergencyCreationViewModel(ICommandHandler<StartEmergency> startEmergencyHandler)
    {
        _startEmergencyHandler = startEmergencyHandler;
        SelectedEmergencyType = EmergencyTypes.FirstOrDefault();
    }

    private void ResetCreateEmergencyFields()
    {
        SelectedEmergencyType = EmergencyTypes.FirstOrDefault();
        EmergencyMessage = string.Empty;
        LocationDescription = string.Empty;
        Latitude = 0;
        Longitude = 0;
        IsEmailSelected = true;
        SendEmail = IsEmailSelected;
        IsSmsSelected = true;
        SendSms = IsSmsSelected;
        IsMessengerSelected = false;
        SendMessenger = IsMessengerSelected;
        AttachPersonalInfo = true;
        AttachMedicalInfo = true;
        AttachGps = true;
        AttachedPicturePath = string.Empty;
        AttachedDocumentPath = string.Empty;
        IsGroupSelected = true;
        IsSingleSelected = false;
    }

    [RelayCommand]
    private void PreviousEmergencyType()
    {
        EmergencyType[] values = Enum.GetValues<EmergencyType>();
        int currentIndex = Array.IndexOf(values, SelectedEmergencyType);
        int previousIndex = currentIndex - 1;

        if (previousIndex < 0)
            previousIndex = values.Length - 1;

        SelectedEmergencyType = values[previousIndex];
        UpdateEmergencyMessage();
    }

    private void UpdateEmergencyMessage()
    {
        EmergencyMessage = GetDefaultMessage(SelectedEmergencyType);
    }

    [RelayCommand]
    private void NextEmergencyType()
    {
        EmergencyType[] values = Enum.GetValues<EmergencyType>();
        int currentIndex = Array.IndexOf(values, SelectedEmergencyType);
        int nextIndex = currentIndex + 1;

        if (nextIndex >= values.Length)
            nextIndex = 0;

        SelectedEmergencyType = values[nextIndex];
        UpdateEmergencyMessage();
    }

    /// <summary>
    /// Command to add or remove a picture attachment to the emergency.
    /// </summary>
    /// <remarks>
    /// This command opens a file picker allowing the user to select an image file
    /// to be attached to the emergency notification. If a picture is already attached,
    /// this command will remove it instead.
    /// </remarks>
    [RelayCommand]
#if UNIT_TESTS
    private Task TogglePictureAsync() => Task.CompletedTask;
#else
    private async Task TogglePictureAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(AttachedPicturePath))
            {
                // Remove the attached picture
                AttachedPicturePath = string.Empty;
                return;
            }

            FileResult? photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                AttachedPicturePath = photo.FullPath;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking photo: {ex.Message}");
        }
    }
#endif

    /// <summary>
    /// Command to add or remove a document attachment to the emergency.
    /// </summary>
    /// <remarks>
    /// This command opens a file picker allowing the user to select a document file
    /// to be attached to the emergency notification. If a document is already attached,
    /// this command will remove it instead.
    /// </remarks>
    [RelayCommand]
#if UNIT_TESTS
    private Task ToggleDocumentAsync() => Task.CompletedTask;
#else
    private async Task ToggleDocumentAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(AttachedDocumentPath))
            {
                // Remove the attached document
                AttachedDocumentPath = string.Empty;
                return;
            }

            FileResult? document = await FilePicker.PickAsync();
            if (document != null)
            {
                AttachedDocumentPath = document.FullPath;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking document: {ex.Message}");
        }
    }
#endif

    /// <summary>
    /// Command to toggle the personal information attachment.
    /// </summary>
    /// <remarks>
    /// This command toggles the AttachPersonalInfo property, allowing users to
    /// include or exclude their personal information from the emergency notification.
    /// </remarks>
    [RelayCommand]
    private void TogglePersonalInfo()
    {
        AttachPersonalInfo = !AttachPersonalInfo;
    }

    /// <summary>
    /// Command to toggle the medical information attachment.
    /// </summary>
    /// <remarks>
    /// This command toggles the AttachMedicalInfo property, allowing users to
    /// include or exclude their medical information from the emergency notification.
    /// </remarks>
    [RelayCommand]
    private void ToggleMedicalInfo()
    {
        AttachMedicalInfo = !AttachMedicalInfo;
    }

    /// <summary>
    /// Command to toggle the GPS location attachment.
    /// </summary>
    /// <remarks>
    /// This command toggles the AttachGps property, allowing users to
    /// include or exclude their current GPS location from the emergency notification.
    /// </remarks>
    [RelayCommand]
    private void ToggleGps()
    {
        AttachGps = !AttachGps;
    }

    [RelayCommand]
    private void ToggleGroup()
    {
        IsGroupSelected = !IsGroupSelected;
        if (IsGroupSelected)
            IsSingleSelected = false;
    }

    [RelayCommand]
    private void ToggleSingle()
    {
        IsSingleSelected = !IsSingleSelected;
        if (IsSingleSelected)
            IsGroupSelected = false;
    }

    [RelayCommand]
    private void ToggleEmail()
    {
        IsEmailSelected = !IsEmailSelected;
        SendEmail = IsEmailSelected;
    }

    [RelayCommand]
    private void ToggleSms()
    {
        IsSmsSelected = !IsSmsSelected;
        SendSms = IsSmsSelected;
    }

    [RelayCommand]
    private void ToggleMessenger()
    {
        IsMessengerSelected = !IsMessengerSelected;
        SendMessenger = IsMessengerSelected;
    }

    [RelayCommand]
    private void AddMessage()
    {
        // Add a new message template or clear the current message
        EmergencyMessage = string.Empty;
    }

    [RelayCommand]
    private void RemoveMessage()
    {
        // Remove the current message and use default
        EmergencyMessage = GetDefaultMessage(SelectedEmergencyType);
    }

    /// <summary>
    /// Command to create the emergency with all configured parameters.
    /// </summary>
    /// <remarks>
    /// This command processes all the emergency creation parameters and executes
    /// the StartEmergency command. It creates the emergency with attachments,
    /// location information, and contact preferences as configured by the user.
    /// </remarks>
    [RelayCommand]
    private async Task CreateEmergencyAsync()
    {
        try
        {
            IsLoading = true;

            // Create location object
            var location = new Domain.Common.Location(Latitude, Longitude, LocationDescription);

            // Create affected areas (default 5km radius)
            var affectedAreas = new List<Area>
            {
                Area.FromLocation(location, 5000)
            };

            // Create attachments
            var attachments = new EmergencyAttachments
            {
                IncludePersonalInfo = AttachPersonalInfo,
                IncludeMedicalInfo = AttachMedicalInfo,
                IncludeGpsLocation = AttachGps,
                PicturePath = AttachedPicturePath,
                DocumentPath = AttachedDocumentPath,
                SendEmail = SendEmail,
                SendSms = SendSms,
                SendMessenger = SendMessenger
            };

            // Use custom message or default based on emergency type
            string message = string.IsNullOrWhiteSpace(EmergencyMessage)
                ? GetDefaultMessage(SelectedEmergencyType)
                : EmergencyMessage;

            var command = new StartEmergency(
                Guid.NewGuid(), // TODO: Replace with actual user ID from auth
                SelectedEmergencyType,
                location,
                affectedAreas,
                message,
                attachments
            );

            await _startEmergencyHandler.Handle(command, CancellationToken.None);

            // Raise the EmergencyCreated event
            EmergencyCreated?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating emergency: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            ResetCreateEmergencyFields();
        }
    }

    /// <summary>
    /// Command to cancel the emergency creation process.
    /// </summary>
    /// <remarks>
    /// This command raises the Cancelled event, signaling to the parent ViewModel
    /// that the emergency creation should be cancelled and the popup closed.
    /// </remarks>
    [RelayCommand]
    private void Cancel()
    {
        ResetCreateEmergencyFields();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
