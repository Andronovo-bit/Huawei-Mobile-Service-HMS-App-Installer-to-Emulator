using HuaweiHMSInstaller.Pages;

namespace HuaweiHMSInstaller;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void OnInstallButtonClicked(object sender, EventArgs e)
	{
        Application.Current.MainPage.Navigation.PushModalAsync(new DownloadandInstallPage(), true);
    }
}

