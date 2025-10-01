using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;

namespace Templanza.Mobile.Services;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // HttpClient según plataforma (Android emulador usa el host de loopback 10.0.2.2)
        builder.Services.AddSingleton(sp => new HttpClient
        {
            BaseAddress = new Uri(
                DeviceInfo.Platform == DevicePlatform.Android
                    ? "https://10.0.2.2:7069/" 
                    : "https://localhost:7069") 
        });

        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<BlendsService>();
        builder.Services.AddSingleton<UsersService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
