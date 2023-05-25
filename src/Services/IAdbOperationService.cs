using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdvancedSharpAdbClient.DeviceCommands.PackageManager;

namespace HuaweiHMSInstaller.Services
{
    public interface IAdbOperationService
    {
        public bool CheckAdbServer();
        public Task DownloadAdbFromInternetAsync(IProgress<float> progress = null);
        public Task<List<DeviceData>> GetDevices();
        public Task DownloadApkFromInternetAsync(string apkUrl, string apkName, IProgress<float> progress = null);
    }
}
