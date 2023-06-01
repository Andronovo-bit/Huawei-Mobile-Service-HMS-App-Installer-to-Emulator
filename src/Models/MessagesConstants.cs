using HuaweiHMSInstaller.Services;
using LocalizationResourceManager.Maui;

namespace HuaweiHMSInstaller.Models
{
    public static class AdbMessagesConst
    {
        private static ILocalizationResourceManager _localizationResourceManager { get => Services.ServiceProvider.GetService<ILocalizationResourceManager>(); }

        // Use constants instead of properties for the message keys
        private const string DownloadingADBDriverKey = "downloading_adb_driver";
        private const string InstallingADBDriverKey = "installing_adb_driver";
        private const string DownloadingHMSCoreKey = "downloading_hms_core";
        private const string InstallingHMSCoreKey = "installing_hms_core";
        private const string DownloadingHMSAppGalleryKey = "downloading_appgallery";
        private const string InstallingHMSAppGalleryKey = "installing_appgallery";
        private const string DownloadingGameKey = "downloading_game";
        private const string InstallingGameKey = "installing_game";
        private const string FinishingKey = "finishing";

        // Use a helper method to get the localized value for a given key
        private static string GetLocalizedValue(string key) => _localizationResourceManager.GetValue(key);

        // Use properties to get the localized messages
        public static string DownloadingADBDriver => GetLocalizedValue(DownloadingADBDriverKey);
        public static string InstallingADBDriver => GetLocalizedValue(InstallingADBDriverKey);
        public static string DownloadingHMSCore => GetLocalizedValue(DownloadingHMSCoreKey);
        public static string InstallingHMSCore => GetLocalizedValue(InstallingHMSCoreKey);
        public static string DownloadingHMSAppGallery => GetLocalizedValue(DownloadingHMSAppGalleryKey);
        public static string InstallingHMSAppGallery => GetLocalizedValue(InstallingHMSAppGalleryKey);
        public static string DownloadingGame => GetLocalizedValue(DownloadingGameKey);
        public static string InstallingGame => GetLocalizedValue(InstallingGameKey);
        public static string Finishing => GetLocalizedValue(FinishingKey);

        // Use a helper method to initialize the dictionary with the message keys and values
        private static Dictionary<string, bool> InitializeMessages()
        {
            var messages = new Dictionary<string, bool>
            {
                { DownloadingADBDriver, true },
                { InstallingADBDriver, true },
                { DownloadingHMSCore, true },
                { DownloadingHMSAppGallery, true },
                { DownloadingGame, true },
                { InstallingHMSCore, true },
                { InstallingHMSAppGallery, true },
                { InstallingGame, true },
                { Finishing, true }
            };
            return messages;
        }

        // Use a readonly field instead of a property for the dictionary
        public readonly static Dictionary<string, bool> Messages = InitializeMessages();
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
