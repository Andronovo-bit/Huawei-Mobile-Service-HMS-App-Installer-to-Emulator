using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Services;

namespace HmsInstallerTest
{
    public class InternetCheckUnitTest
    {
        [Fact]
        public async Task CheckForInternetConnectionAsync_ReturnsTrue_WhenInternetIsAvailable()
        {
            // Act
            var result = await NetworkUtils.CheckForInternetConnectionAsync();

            // Assert
            Assert.True(result);
        }
    }
}