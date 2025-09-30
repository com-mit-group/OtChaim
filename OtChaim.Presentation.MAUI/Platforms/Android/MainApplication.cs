using Android.App;
using Android.Runtime;

using MauiApplication = Microsoft.Maui.MauiApplication;

namespace OtChaim.Presentation.MAUI;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
