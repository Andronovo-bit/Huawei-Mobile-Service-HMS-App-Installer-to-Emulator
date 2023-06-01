using LocalizationResourceManager.Maui;
using System.Globalization;

namespace HuaweiHMSInstaller.Models
{
    public static class AdbMessagesConst
    {
        private static ILocalizationResourceManager _localizationResourceManager { get => Services.ServiceProvider.GetService<ILocalizationResourceManager>(); }
        // Use a helper method to get the localized value for a given key
        private static string GetLocalizedValue(string key) => _localizationResourceManager.GetValue(key);
        private static CultureInfo CurrentLanguage = _localizationResourceManager.CurrentCulture;

        public static string DownloadingADBDriver => GetLocalizedValue("downloading_adb_driver");
        public static string InstallingADBDriver => GetLocalizedValue("installing_adb_driver");
        public static string DownloadingHMSCore => GetLocalizedValue("downloading_hms_core");
        public static string InstallingHMSCore => GetLocalizedValue("installing_hms_core");
        public static string DownloadingHMSAppGallery => GetLocalizedValue("downloading_appgallery");
        public static string InstallingHMSAppGallery => GetLocalizedValue("installing_appgallery");
        public static string DownloadingGame => GetLocalizedValue("downloading_game");
        public static string InstallingGame => GetLocalizedValue("installing_game");
        public static string Finishing => GetLocalizedValue("finishing");

        // Use a helper method to initialize the dictionary with the message keys and values
        public static Dictionary<string, bool> InitializeMessages()
        {
            Dictionary<string, bool> messages;

            if (CurrentLanguage == _localizationResourceManager.CurrentCulture)
            {
                messages = new Dictionary<string, bool>
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
           
            messages = new Dictionary<string, bool>
            {
                { GetLocalizedValue("downloading_adb_driver"), true },
                { GetLocalizedValue("installing_adb_driver"), true },
                { GetLocalizedValue("downloading_hms_core"), true },
                { GetLocalizedValue("downloading_appgallery"), true },
                { GetLocalizedValue("downloading_game"), true },
                { GetLocalizedValue("installing_hms_core"), true },
                { GetLocalizedValue("installing_appgallery"), true },
                { GetLocalizedValue("installing_game"), true },
                { GetLocalizedValue("finishing"), true }
            };
            CurrentLanguage = _localizationResourceManager.CurrentCulture;
            return messages;


        }


    }

    public static class HmsInfoMessagesConst{

        private static ILocalizationResourceManager _localizationResourceManager { get => Services.ServiceProvider.GetService<ILocalizationResourceManager>(); }
        // Use a helper method to get the localized value for a given key
        private static string GetLocalizedValue(string key) => _localizationResourceManager.GetValue(key);
        // Use a helper method to initialize the dictionary with the message keys and values
        public static string[] InitializeMessages()
        {
            var messages = new string[]
            {
                GetLocalizedValue("hms_info_message_1"),
                GetLocalizedValue("hms_info_message_2"),
                GetLocalizedValue("hms_info_message_3"),
                GetLocalizedValue("hms_info_message_4"),
                GetLocalizedValue("hms_info_message_5")
            };
            return messages;
        }
    }
}
