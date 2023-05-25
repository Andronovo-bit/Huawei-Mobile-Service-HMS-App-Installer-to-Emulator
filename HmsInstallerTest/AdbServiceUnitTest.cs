using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace HmsInstallerTest
{
    public class AdbServiceUnitTest
    {
        private Mock<HttpClient> _httpClient = new();
        private Mock<IAdbOperationService> _AdbOperationService = new Mock<IAdbOperationService>();

        [Fact]
        public void CheckAdbServer_ReturnsTrue_WhenServerIsRunning()
        {
            // Arrange
            var service = _AdbOperationService.Object;

            // Act
            var result = service.CheckAdbServer();

            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task DownloadAdbFromInternet_DownloadsAndExtractsFile_WhenUrlIsValid()
        {
            // Arrange
            var service = _AdbOperationService.Object;
            const string adbFolder = "adb_server";

            // Act
            await service.DownloadAdbFromInternetAsync();

            // Assert
            // You can use System.IO methods to check if the file and folder exist
            Assert.True(File.Exists(adbFolder + "\\platform-tools\\adb.exe"));
        }


        [Fact]
        public async Task GetDevice_ReturnsDevice_WhenDeviceIsConnected()
        {
            // Arrange
            var service = _AdbOperationService.Object;
            // You may need to mock the AdbClient class to simulate a connected device

            // Act
            var device = await service.GetDevices();

            // Assert
            Assert.NotNull(device);
        }
    }


}