using HuaweiHMSInstaller.Pages;

namespace HuaweiHMSInstaller;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
        Routing.RegisterRoute(nameof(DownloadandInstallPage), typeof(DownloadandInstallPage));
        Routing.RegisterRoute(nameof(ThanksPage), typeof(ThanksPage));

    }
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.MinimumWidth = 600;
        window.MinimumHeight = 600;
        return window;
    }

}
