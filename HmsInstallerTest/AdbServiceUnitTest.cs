using HuaweiHMSInstaller.Services;

namespace HmsInstallerTest
{
    public class AdbServiceUnitTest
    {
        [Fact]
        public void CheckAdbServer_ReturnsTrue_WhenServerIsRunning()
        {
            // Arrange
            var service = new AdbOperationService();

            // Act
            var result = service.CheckAdbServer();

            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task DownloadAdbFromInternet_DownloadsAndExtractsFile_WhenUrlIsValid()
        {
            // Arrange
            var service = new AdbOperationService();
            const string adbFolder = "adb_server";

            // Act
            await service.DownloadAdbFromInternet();

            // Assert
            // You can use System.IO methods to check if the file and folder exist
            Assert.True(File.Exists(adbFolder + "\\platform-tools\\adb.exe"));
        }


        [Fact]
        public async Task GetDevice_ReturnsDevice_WhenDeviceIsConnected()
        {
            // Arrange
            var service = new AdbOperationService();
            // You may need to mock the AdbClient class to simulate a connected device

            // Act
            var device = await service.GetDevice();

            // Assert
            Assert.NotNull(device);
        }
    }


}