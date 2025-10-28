using Templanza.Mobile.Services;

namespace Templanza.Mobile;

public partial class App : Application
{
    private readonly AuthService _auth;

    public App(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    protected override async void OnStart()
    {
        try
        {
            await _auth.RestoreTokenAsync();
        }
        catch { /* noop */ }

        base.OnStart();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Templanza.Mobile" };
    }
}
