using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public const string ConnectionYourADBDevices = "Connection Your ADB Devices (Emulator)";
        public const string Finishing = "Finishing";

        public readonly static Dictionary<string,bool> Messages = 
        new Dictionary<string, bool>
        {
            { DownloadingADBDriver, true },
            { InstallingADBDriver, true },
            { DownloadingHMSCore, true },
            { DownloadingHMSAppGallery, true },
            { DownloadingGame, true },
            { ConnectionYourADBDevices, true },
            { InstallingHMSCore, true },
            { InstallingHMSAppGallery, true },
            { InstallingGame, true },
            { Finishing, true },
        };

    }
}
