using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using HuaweiHMSInstaller.Helper;

namespace HuaweiHMSInstaller.Services
{
    public class AdbOperationService : IAdbOperationService
    {
        // Use constants or configuration values for the URL and the destination folder
        private const string AdbUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
        private const string AdbFolder = "adb_server";

        // Use HttpClientFactory or dependency injection to create HttpClient instances
        private readonly HttpClient _client;
        private readonly AdbClient _adbClient;

        public IProgress<float> Progress { get; set; }


        public AdbOperationService(HttpClient client)
        {
            _adbClient = new AdbClient();
            _client = client ?? throw new ArgumentNullException(nameof(client));

        }

        public bool CheckAdbServer()
        {
            return AdbServer.Instance.GetStatus().IsRunning;
        }

        // A public method that downloads and extracts the adb file
        public async Task DownloadAdbFromInternetAsync(IProgress<float> progress = null)
        {
            // Get the file name from the URL
            string fileName = Path.GetFileName(AdbUrl);
            // Create a file stream to store the downloaded data
            using (var file = new FileStream(Path.Combine(Path.GetTempPath(), fileName), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                // Download the file using the custom extension method
                await _client.DownloadAsync(AdbUrl, file, Progress);
            }

            // Extract the file to the destination folder
            ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), fileName), Path.Combine(Path.GetTempPath(), AdbFolder),true);
            // Delete the file
            File.Delete(Path.Combine(Path.GetTempPath(), fileName));
        }


        public async Task<List<DeviceData>> GetDevices()
        {
            var devices = await _adbClient.GetDevicesAsync();
            if (devices.Count == 0)
            {
                return null;
            }
            return devices;
        }
    }
}
