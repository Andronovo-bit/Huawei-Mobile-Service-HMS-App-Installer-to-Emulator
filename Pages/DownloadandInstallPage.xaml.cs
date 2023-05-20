namespace HuaweiHMSInstaller.Pages;

public partial class DownloadandInstallPage : ContentPage
{
	public DownloadandInstallPage()
	{
		InitializeComponent();
        //Add async timer not use Device.StartTimer because it is not supported in .NET MAUI 
        Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
		{
            Application.Current.MainPage.Navigation.PushModalAsync(new ThanksPage(), true);

            return true;
		});


	}

}