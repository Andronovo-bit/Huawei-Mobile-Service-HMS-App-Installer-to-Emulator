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
}
