using OtChaim.Presentation.MAUI.ViewModels.Settings;

namespace OtChaim.Presentation.MAUI.Pages.Settings;

public partial class UserInfoPage : ContentView
{
    private bool _isLoaded;

    public UserInfoPage(UserInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;

        if (BindingContext is UserInfoViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
