using HuaweiHMSInstaller.Integrations;
using HuaweiHMSInstaller.Models.MappingModels;
using Newtonsoft.Json;

namespace HmsInstallerTest
{
    public class AppGalleryIntegrationUnitTest 
    {
        // Define a reusable HttpClient instance
        private IHttpClientFactory _client;


        public AppGalleryIntegrationUnitTest(IHttpClientFactory client)
        {
            _client = client;
        }



        // Define a test method for the function with valid parameters
        [Fact]
        public async Task GetResponseAsync_WithValidParameters_ReturnsExpectedContent()
        {
            // Arrange

            const string keyword = "summoner";
            const string locale = "tr";
            var appGalleryIntegration = new AppGalleryIntegration(_client);

            // Act
            var result = await appGalleryIntegration.SearchAppInAppGalleryAsync(keyword, locale);

            var mappingResult = JsonConvert.DeserializeObject<AppGallerySearchResult>(result);


            // Assert
            Assert.NotNull(result);
            Assert.NotNull(mappingResult);
            Assert.Equal(keyword, mappingResult.keyword);
        }

        [Fact]
        public async Task Should_Return_Valid_Content_When_Keyword_And_Locale_Are_Valid()
        {
            // Arrange
            var keyword = "Summoners War: Chronicles";
            var locale = "tr";
            var appGalleryIntegration = new AppGalleryIntegration(_client);

            // Act
            var content = await appGalleryIntegration.AdvancedSearchAppInAppGalleryAsync(keyword, locale);
            var mappingResult = JsonConvert.DeserializeObject<AppGalleryAdvancedSearchResult>(content);

            // Assert
            Assert.NotNull(content);
            Assert.Contains(keyword, content);
            Assert.Contains(locale, content);
            Assert.True(mappingResult?.count > 0);
            var hasGameName = mappingResult.layoutData.Any(t => t.dataList.Any(k => k.name == keyword));
            Assert.True(hasGameName);
        }

        // Define a method to test the GetDetailAppInAppGalleryAsync method
        [Fact]
        public async Task GetDetailAppInAppGalleryAsync_ShouldReturnSuccess()
        {
            // Arrange
            // Create an instance of the class that contains the method to test
            var appGallery = new AppGalleryIntegration(_client);

            // Define the input parameters for the method
            var appId = "C102059395";
            var locale = "tr";

            // Act
            // Call the method and get the content
            var content = await appGallery.GetDetailAppInAppGalleryAsync(appId, locale);
            var mappingResult = JsonConvert.DeserializeObject<AppGalleryAppDetailResult>(content);

            // Assert

            // Verify that the result content is not null or empty
            Assert.NotNull(content);
            var hasGameName = mappingResult.layoutData.Any(t => t.dataList.Any(k => k.appid == appId));
            Assert.True(hasGameName);
        }


    }
}
