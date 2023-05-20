using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedSharpAdbClient;

namespace HuaweiHMSInstaller.Services
{
    public class AdbOperationService
    {
        private readonly AdbClient _adbClient;

        public AdbOperationService()
        {
            _adbClient = new AdbClient();
        }

        public bool CheckAdbServer()
        {
            return AdbServer.Instance.GetStatus().IsRunning;
        }

        public async Task DownloadAdbFromInternet()
        {
            // Use a constant or a configuration value for the URL
            const string adbUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
            // Use a constant or a configuration value for the destination folder
            const string adbFolder = "adb_server";
            // Call the helper method to download and extract the file
            await DownloadAndExtractFileAsync(adbUrl, adbFolder);
        }

        // A helper method that downloads and extracts a file from a given URL to a given folder
        private async Task DownloadAndExtractFileAsync(string url, string folder)
        {
            // Use HttpClientFactory or dependency injection to create HttpClient instances
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Get the file name from the URL
                    string fileName = Path.GetFileName(url);
                    // Download the file as a byte array
                    var response = await client.GetAsync(url);
                    var content = await response.Content.ReadAsByteArrayAsync();
                    // Write the file to disk
                    File.WriteAllBytes(fileName, content);
                    // Extract the file to the destination folder
                    ZipFile.ExtractToDirectory(fileName, folder);
                    // Delete the file
                    File.Delete(fileName);
                }
                catch (Exception e)
                {
                    // Handle or log the exception as needed
                    Console.WriteLine(e.Message);
                }
            }
        }


        public async Task<DeviceData> GetDevice()
        {
            var devices = await _adbClient.GetDevicesAsync();
            if (devices.Count == 0)
            {
                return null;
            }

            var device = devices.FirstOrDefault();
            return device;
        }
    }
}
