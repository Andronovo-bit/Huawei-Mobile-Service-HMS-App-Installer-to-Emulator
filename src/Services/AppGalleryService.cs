using HuaweiHMSInstaller.Integrations;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Models.MappingModels;
using Newtonsoft.Json;

namespace HuaweiHMSInstaller.Services
{
    public class AppGalleryService: IAppGalleryService
    {
        // Dependency injection to create Appgallery instance
        private readonly IAppGalleryIntegration _appGalleryIntegration;

        public AppGalleryService(IAppGalleryIntegration appGalleryIntegration)
        {
            _appGalleryIntegration = appGalleryIntegration;
        }

        public async Task<AppGalleryAdvancedSearchResult> SearchAppGalleryApp(string keyword)
        {
            //get system local language
            var language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var result = await _appGalleryIntegration.AdvancedSearchAppInAppGalleryAsync(keyword,language);
            var mappingResult = JsonConvert.DeserializeObject<AppGalleryAdvancedSearchResult>(result);
            return mappingResult;
        }


        public async Task<AppGalleryAppDetailResult> GetAppDetail(string appId)
        {
            var language = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            var result = await _appGalleryIntegration.GetDetailAppInAppGalleryAsync(appId, language);
            var mappingResult = JsonConvert.DeserializeObject<AppGalleryAppDetailResult>(result);
            return mappingResult;
        }

        public async Task<bool> CheckAppGalleryServiceAsync() => await _appGalleryIntegration.CheckBaseUrlAsync();
    }
}
