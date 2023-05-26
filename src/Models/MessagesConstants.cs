namespace HuaweiHMSInstaller.Models
{
    public static class AdbMessagesConst
    {
        public const string DownloadingADBDriver = "Downloading the ADB Driver";
        public const string InstallingADBDriver = "Installing the ADB Driver";
        public const string DownloadingHMSCore = "Downloading the HMS Core";
        public const string InstallingHMSCore = "Installing HMS Core";
        public const string DownloadingHMSAppGallery = "Downloading the HMS AppGallery";
        public const string InstallingHMSAppGallery = "Installing the HMS AppGallery";
        public const string DownloadingGame = "Downloading Game";
        public const string InstallingGame = "Installing Game";
        public const string Finishing = "Finishing";

        public readonly static Dictionary<string,bool> Messages = 
        new Dictionary<string, bool>
        {
            { DownloadingADBDriver, true },
            { InstallingADBDriver, true },
            { DownloadingHMSCore, true },
            { DownloadingHMSAppGallery, true },
            { DownloadingGame, true },
            { InstallingHMSCore, true },
            { InstallingHMSAppGallery, true },
            { InstallingGame, true },
            { Finishing, true },
        };

    }

    public static class HmsInfoMessagesConst{
        public static readonly string[] hmsInfoMessage = new string[]
        {
            "Huawei Mobile Services (HMS) is a suite of mobile services developed by Huawei for its smartphones and other devices.",
            "HMS is available in over 170 countries and regions, and it is used by over 700 million users worldwide.",
            "Huawei is constantly expanding the HMS ecosystem, and it is working to bring new and innovative services to its users.",
            "HMS is designed to be a secure and reliable alternative to Google Mobile Services (GMS), which is the suite of mobile services that is used by most Android smartphones.",
            "HMS includes a variety of services, such as the AppGallery app store, the Huawei Cloud cloud storage service, and the Huawei Health fitness tracking service.",
        };
    }
}
