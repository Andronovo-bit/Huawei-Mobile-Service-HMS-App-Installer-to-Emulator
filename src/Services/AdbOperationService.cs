using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Exceptions;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using Microsoft.Extensions.Options;
using static AdvancedSharpAdbClient.DeviceCommands.PackageManager;

namespace HuaweiHMSInstaller.Services
{
    public class AdbOperationService : IAdbOperationService
    {
        // Declare a boolean flag to store the network availability
        private bool _isNetworkAvailable = true;
        // Use constants or configuration values for the URL and the destination folder
        private const string AdbUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
        private const string AdbFolder = "adb_server";
        private const string ADB = "adb";
        private string adbPath;
        // Dependency injection to create HttpClient, AdbClient, Options instances
        private readonly IHttpClientFactory _httpClient;
        private AdbClient _adbClient;
        private readonly GlobalOptions _options;

        public AdbOperationService(
                            IHttpClientFactory httpClient, 
                            IOptions<GlobalOptions> options)
        {
            _options = options.Value;
            _httpClient = httpClient;
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
            adbPath = Path.Combine(_options.ProjectOperationPath, AdbFolder, "platform-tools", "adb.exe");
        }
        public async Task CreateAdbClient()
        {
            // Try to get the existing ADB process and port number.
            var adbServerStatus = AdbServer.Instance.GetStatus().IsRunning;
            var adbProcess = WorkingProcessAndPort.GetProcessByName(ADB);
            var adbPortNumber = adbProcess != null ? await WorkingProcessAndPort.GetProcessPortNumberAsync(adbProcess.Id) : null;

            // If the ADB process or port number is null, start a new server and get them again.
            if (adbProcess == null && !adbServerStatus)
            {
                AdbServer.Instance.StartServer(adbPath, true);
                adbProcess = WorkingProcessAndPort.GetProcessByName(ADB);
                adbPortNumber = await WorkingProcessAndPort.GetProcessPortNumberAsync(adbProcess.Id);
            }

            // If the ADB port number is different from the default one, create a new client with a custom endpoint and socket factory.
            if (adbPortNumber != null && adbPortNumber != AdbClient.AdbServerPort)
            {
                var endPoint = new IPEndPoint(IPAddress.Loopback, (int)adbPortNumber);
                var adbSocketFactory = Factories.AdbSocketFactory;
                _adbClient = new AdbClient(endPoint, adbSocketFactory);
            }
            else
            {
                // Otherwise, create a new client with the default settings.
                _adbClient = new AdbClient();
            }
        }

        public bool CheckAdbServer()
        {
            try
            {
                return AdbServer.Instance.GetStatus().IsRunning;

            }catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
            
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
                await _httpClient.DownloadAsync(AdbUrl, file, progress);
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
                var defaultDevice = await GetDevicesDefaultAsync();
                if(defaultDevice) devices = await _adbClient.GetDevicesAsync();
            }
            return devices;
        }
        public async Task DownloadApkFromInternetAsync(string apkUrl, string apkName, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create a file stream to store the downloaded data
                using (var file = new FileStream(Path.Combine(_options.ProjectOperationPath, apkName), FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    // Add try-catch block to handle NetworkErrorException
                    try
                    {
                        // Download the file using the custom extension method
                        await _httpClient.DownloadAsync(apkUrl, file, progress, cancellationToken);

                        // Check the network availability flag before completing the download
                        if (!_isNetworkAvailable)
                        {
                            // Cancel the download and notify the user
                            Debug.WriteLine("Internet connection lost during download.");
                            return;
                        }
                    }
                    catch (TaskCanceledException ex) // The request was canceled due to the timeout or the token
                    {
                        // Check if the cancellation was requested by the token
                        await file.DisposeAsync();
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        // Cancel the download and notify the user
                        Debug.WriteLine("Network error occurred: " + ex.Message);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while downloading the APK: " + ex.Message);
            }
        }
        public void InstallApkToDevice(string packageFilePath, ProgressHandler installProgressChanged, DeviceData device)
        {
            // Create a new package manager instance with the ADB client and the device.
            PackageManager manager;
            try
            {
                manager = new PackageManager(_adbClient, device);
            }
            catch (AdbException ex)
            {
                var deviceIPandPort = device.Serial.Split(':');
                _adbClient.Connect(deviceIPandPort[0], Convert.ToInt32(deviceIPandPort[1]));
                manager = new PackageManager(_adbClient, device);
            }
            manager.InstallProgressChanged += installProgressChanged;

            //var huaweiPackages = manager.Packages.Where(t => t.Key.Contains("huawei", StringComparison.InvariantCultureIgnoreCase));

            // Install the APK
            //Try to install the package file on the device with a maximum number of retries.
            int maxRetries = 3;
            int retryCount = 0;
            bool installed = false;
            while (!installed && retryCount < maxRetries)
            {
                try
                {
                    manager.InstallPackage(packageFilePath, true);
                    installed = true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("INSTALL_FAILED_ALREADY_EXISTS"))
                    {
                        installed = true;
                        continue;
                    }

                    if(ex.Message.Contains("INSTALL_FAILED_UPDATE_INCOMPATIBLE"))
                    {
                        //TODO : Find uninstall package name using adb command
                        manager.UninstallPackage("com.huawei.hwid");
                    }

                    // Catch any exceptions that occur during installation and restart the ADB server.
                    Debug.WriteLine(ex.Message);
                    RestartAdbServer().ConfigureAwait(false).GetAwaiter().GetResult();
                    ConnectToDeviceAsync(device).ConfigureAwait(false).GetAwaiter().GetResult();
                    retryCount++;
                }
            }

            // If the APK was not installed, throw an exception.
            if (!installed)
            {
                var packageName = Path.GetFileNameWithoutExtension(packageFilePath);
                
                throw new Exception($"The APK ({packageName}) could not be installed on the device.");
            }
            
        }

        private async Task RestartAdbServer()
        {
            await CreateAdbClient();
        }
        private async Task ConnectToDeviceAsync(DeviceData device)
        {
            // Connect to the device with its IP address.
            try
            {
                if (_adbClient.GetDevices().Count == 0)
                    await _adbClient.ConnectAsync(IPAddress.Loopback.ToString(), Convert.ToInt32(device.Serial.Split(':')[1]));
            }
            catch (SocketException socketEx)
            {
                Debug.WriteLine(socketEx.Message);
            }
        }
        private async Task<bool> GetDevicesDefaultAsync()
        {
            var defaultPorts = DefaultEmulatorAdbPorts.GetAllDefaultEmulatorPorts();

            foreach (var defaultPort in defaultPorts)
            {
                var adbDevice = await _adbClient.ConnectAsync(IPAddress.Loopback.ToString(), defaultPort);
                if (!adbDevice.Contains("(10061)"))
                {
                    return true;
                }
               
            }

            return false;
        }
        // Define a handler for the NetworkAvailabilityChanged event
        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            // Update the flag based on the network availability
            _isNetworkAvailable = e.IsAvailable;
        }
    }
}
