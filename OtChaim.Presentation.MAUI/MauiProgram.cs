using Microsoft.Extensions.Logging;
using OtChaim.Application;
using OtChaim.Application.Services;
using OtChaim.Persistence;
using OtChaim.Presentation.MAUI.Pages.Settings;
using OtChaim.Presentation.MAUI.Pages.Tool;
using OtChaim.Presentation.MAUI.ViewModels.Settings;
using OtChaim.Presentation.MAUI.ViewModels.Tool;

namespace OtChaim.Presentation.MAUI;

/// <summary>
/// Provides configuration and startup logic for the OtChaim MAUI application.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application.
    /// </summary>
    /// <returns>The configured <see cref="MauiApp"/> instance.</returns>
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register serviceProvider
        builder.Services.AddPersistence(useInMemory: true);
        builder.Services.AddEventAggregator();

        // Register application serviceProvider
        builder.Services.AddSingleton<EmergencyDataService>();
        builder.Services.AddSingleton<UserProfileService>();

        // Register view models
        builder.Services.AddTransient<EmergencyDashboardViewModel>();
        builder.Services.AddTransient<ToolTabViewModel>();
        builder.Services.AddTransient<SettingsTabViewModel>();
        builder.Services.AddTransient<EmergencyViewModel>();
        builder.Services.AddTransient<EmergencyCreationViewModel>();
        builder.Services.AddTransient<GroupStatusViewModel>();
        builder.Services.AddTransient<UserInfoViewModel>();
        builder.Services.AddTransient<MedicalInfoViewModel>();
        builder.Services.AddTransient<EmergencyContactsViewModel>();

        // Register pages
        builder.Services.AddTransient<EmergencyDashboardPage>();
        builder.Services.AddTransient<ToolTabPage>();
        builder.Services.AddTransient<SettingsTabPage>();
        builder.Services.AddTransient<EmergencyPage>();
        builder.Services.AddTransient<EmergencyCreationPopup>();
        builder.Services.AddTransient<GroupStatusPage>();
        builder.Services.AddTransient<UserInfoPage>();
        builder.Services.AddTransient<MedicalInfoPage>();
        builder.Services.AddTransient<EmergencyContactsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        MauiApp app = builder.Build();

        // Subscribe event handlers AFTER the full graph is built
        ApplicationDI.SubscribeEventHandlers(app.Services);

        return app;

    }
}
