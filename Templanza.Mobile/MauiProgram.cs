using Microsoft.Extensions.Logging;

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

        builder.Services.AddSingleton(sp => new HttpClient
        {
            BaseAddress = new Uri("https://templanza-api.onrender.com/"),
            Timeout = TimeSpan.FromSeconds(30)
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
