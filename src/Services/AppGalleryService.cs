using AdvancedSharpAdbClient;
using HuaweiHMSInstaller.Integrations;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Models.MappingModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Services
{
    public class AppGalleryService: IAppGalleryService
    {
        // Dependency injection to create HttpClient, AdbClient, Options instances
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GlobalOptions _options;
        private readonly IAppGalleryIntegration _appGalleryIntegration;

        public AppGalleryService(
                            IHttpClientFactory httpClientFactory,
                            IAppGalleryIntegration appGalleryIntegration,
                            IOptions<GlobalOptions> options)
        {
            _options = options.Value;
            _httpClientFactory = httpClientFactory;
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
    }
}
