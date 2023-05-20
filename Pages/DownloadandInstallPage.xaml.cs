using Microsoft.Maui.Dispatching;

namespace HuaweiHMSInstaller.Pages;

public partial class DownloadandInstallPage : ContentPage
{
	public DownloadandInstallPage()
	{

        InitializeComponent();


        //Add async timer not use Device.StartTimer because it is not supported in .NET MAUI 
        //change progressbar value every second

        Dispatcher.DispatchAsync(async () =>
        {
            // Define an array of messages for each progress range
            string[] messages = new string[]
            {
                "Downloading the ADB Driver",
                "Installing the ADB Driver",
                "Downloading the HMS Core",
                "Downloading the HMS AppGallery",
                "Downloadig Game",
                "Connection Your ADB Devices (Emulator)",
                "Installing the HMS Core",
                "Installing the HMS AppGallery",
                "Installing Game",
                "Finishing"

            };

            string[] hmsInfoMessage = new string[]
            {
                "Huawei Mobile Services (HMS) is a suite of mobile services developed by Huawei for its smartphones and other devices.",
                "HMS is available in over 170 countries and regions, and it is used by over 700 million users worldwide.",
                "Huawei is constantly expanding the HMS ecosystem, and it is working to bring new and innovative services to its users.",
                "HMS is designed to be a secure and reliable alternative to Google Mobile Services (GMS), which is the suite of mobile services that is used by most Android smartphones.",
                "HMS includes a variety of services, such as the AppGallery app store, the Huawei Cloud cloud storage service, and the Huawei Health fitness tracking service.",
            };

            // Define an array of progress thresholds for each message
            double[] thresholds = new double[] { 7, 13, 20, 29, 37, 45, 57, 69, 80, 101 };
            double[] thresholdsHmsInfo = new double[] { 20, 40, 60, 80, 101 };

            // Initialize a variable to keep track of the current message index
            int index = 0;
            int indexhmsInfo = 0;

            while (true)
            {
                await Task.Delay(1000);

                // Update the comment label text based on the current progress and message index
                this.commentLabel.Text = messages[index];
                this.HMSInfoLabel.Text = hmsInfoMessage[indexhmsInfo];

                // If the progress exceeds the current threshold, increment the message index
                if (this.progressBar.Progress >= thresholds[index] / 100)
                {
                    index++;
                }
                if (this.progressBar.Progress >= thresholdsHmsInfo[indexhmsInfo] / 100)
                {
                    indexhmsInfo++;
                }

                // Add or remove dots at the end of the comment label text
                if (this.dotAnimation.Text.EndsWith("..."))
                {
                    this.dotAnimation.Text = this.dotAnimation.Text.Substring(0, this.dotAnimation.Text.Length - 3);
                }
                else
                {
                    this.dotAnimation.Text += ".";
                }

                // Update the progress bar value
                this.progressBar.Progress += 0.01;

                // If the progress reaches 1, start the timer and break the loop
                if (this.progressBar.Progress == 1)
                {
                    StartTimer();
                    break;
                }
            }
        });




    }

    public void StartTimer()
    {
        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            Application.Current.MainPage.Navigation.PushModalAsync(new ThanksPage(), true);


            return false;
        });
    }

}