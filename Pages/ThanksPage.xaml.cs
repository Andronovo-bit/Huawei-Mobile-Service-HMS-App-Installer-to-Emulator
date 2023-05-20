namespace HuaweiHMSInstaller.Pages;

public partial class ThanksPage : ContentPage
{
	public ThanksPage()
	{
		InitializeComponent();
	}

    private async void OnInstallButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("MainPage");
    }
}