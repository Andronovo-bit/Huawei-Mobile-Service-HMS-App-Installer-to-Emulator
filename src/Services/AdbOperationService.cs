using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using Microsoft.Extensions.Options;
using static AdvancedSharpAdbClient.DeviceCommands.PackageManager;

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
        private readonly GlobalOptions _options;
        


        public AdbOperationService(HttpClient client, IOptions<GlobalOptions> options)
        {
            _adbClient = new AdbClient();
            _options = options.Value;
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

            //if folder not exist create folder
            Directory.CreateDirectory(_options.ProjectOperationPath);

            // Create a file stream to store the downloaded data
            using (var file = new FileStream(Path.Combine(_options.ProjectOperationPath, fileName), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                // Download the file using the custom extension method
                await _client.DownloadAsync(AdbUrl, file, progress);
            }

            // Extract the file to the destination folder
            ZipFile.ExtractToDirectory(Path.Combine(_options.ProjectOperationPath, fileName), Path.Combine(_options.ProjectOperationPath, AdbFolder),true);
            // Delete the file
            File.Delete(Path.Combine(_options.ProjectOperationPath, fileName));
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
        
        public async Task DownloadApkFromInternetAsync(string apkUrl, string apkName, IProgress<float> progress = null)
        {
            // Create a file stream to store the downloaded data
            using (var file = new FileStream(Path.Combine(_options.ProjectOperationPath, apkName), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                // Download the file using the custom extension method
                await _client.DownloadAsync(apkUrl, file, progress);
            }
        }

    }
}
