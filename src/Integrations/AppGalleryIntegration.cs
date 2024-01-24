using HuaweiHMSInstaller.Helper;
using Microsoft.Extensions.Caching.Memory;
using System.Web;

namespace HuaweiHMSInstaller.Integrations
{
    public class AppGalleryIntegration : IAppGalleryIntegration
    {
        // Define the parameters for the function call
        private const string BASE_URL = "https://web-dre.hispace.dbankcloud.cn";
        private const string APPGALLERY_CLOUD_URL = "https://appgallery.cloud.huawei.com";

        private readonly MemoryCache cache = new(new MemoryCacheOptions());
        private readonly IHttpClientFactory _httpClientFactory;


        public AppGalleryIntegration(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> SearchAppInAppGalleryAsync(string keyword, string locale)
        {
            const string method = "internal.completeSearchWord";
            const string serviceType = "20";
            const string zone = "";
            // Use HttpClientFactory to create an HttpClient instance
            var _httpClient = _httpClientFactory.CreateClient();

            var InterfaceCode = await GetInterfaceCodeAsync();
            // Add headers to the HttpClient instance
            _httpClient.DefaultRequestHeaders.Add("Interface-Code", InterfaceCode);

            // Use UriBuilder and HttpUtility to construct the URL and query string
            var urlBuilder = new UriBuilder(BASE_URL + "/uowap/index");
            var queryParameters = HttpUtility.ParseQueryString(string.Empty);
            queryParameters["method"] = method;
            queryParameters["serviceType"] = serviceType;
            queryParameters["keyword"] = keyword;
            queryParameters["zone"] = zone;
            queryParameters["locale"] = locale;
            urlBuilder.Query = queryParameters.ToString();

            // Use a try-catch block to handle exceptions
            try
            {
                // Generate a cache key from the keyword and locale
                var cacheKey = $"{keyword}_{locale}";

                // Check if the cache contains the key
                if (cache.TryGetValue(cacheKey, out string cachedContent))
                {
                    // Return the cached content
                    return cachedContent;
                }
                else
                {
                    // Use async and await with using to dispose of the HttpResponseMessage object
                    using (var response = await _httpClient.GetAsync(urlBuilder.Uri))
                    {
                        // Throw an exception if the status code is not successful
                        response.EnsureSuccessStatusCode();

                        // Read and return the response content
                        var content = await response.Content.ReadAsStringAsync();

                        // Set the cache options with a one day expiration time
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                        // Store the content in the cache with the key and options
                        cache.Set(cacheKey, content, cacheOptions);

                        // Return the content
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
                // Return the error message
                return ex.Message;
            }
        }

        public async Task<string> AdvancedSearchAppInAppGalleryAsync(string keyword, string locale)
        {
            // Use HttpClientFactory to create an HttpClient instance
            var _httpClient = _httpClientFactory.CreateClient();

            var InterfaceCode = await GetInterfaceCodeAsync();
            // Add headers to the HttpClient instance
            _httpClient.DefaultRequestHeaders.Add("Interface-Code", InterfaceCode);

            // Define the base URL and query parameters as constants
            const string url = BASE_URL + "/uowap/index";
            const string method = "internal.getTabDetail";
            const string serviceType = "20";
            const string reqPageNum = "1";
            const string maxResults = "50";
            const string version = "10.0.0";
            const string zone = "";

            // Encode the keyword for the URI
            var encodedKeyword = Uri.EscapeDataString(keyword);

            // Use UriBuilder and HttpUtility to construct the URL and query string
            var urlBuilder = new UriBuilder(url);
            var queryParameters = HttpUtility.ParseQueryString(string.Empty);
            queryParameters["method"] = method;
            queryParameters["serviceType"] = serviceType;
            queryParameters["reqPageNum"] = reqPageNum;
            queryParameters["uri"] = $"searchApp%7C{encodedKeyword}";
            queryParameters["maxResults"] = maxResults;
            queryParameters["version"] = version;
            queryParameters["zone"] = zone;
            queryParameters["locale"] = locale;
            urlBuilder.Query = queryParameters.ToString();

            // Generate a cache key from the keyword and locale
            var cacheKey = $"adv_{keyword}_{locale}";

            // Check if the cache contains the key
            if (cache.TryGetValue(cacheKey, out string cachedContent))
            {
                // Return the cached content
                return cachedContent;
            }
            else
            {
                // Use async and await with using to dispose of the HttpResponseMessage object
                using (var response = await _httpClient.GetAsync(urlBuilder.Uri))
                {
                    // Throw an exception if the status code is not successful
                    response.EnsureSuccessStatusCode();

                    // Read and return the response content as a string
                    var content = await response.Content.ReadAsStringAsync();

                    // Set the cache options with a one day expiration time
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                    // Store the content in the cache with the key and options
                    cache.Set(cacheKey, content, cacheOptions);

                    // Return the content
                    return content;
                }
            }
        }

        public async Task<string> GetDetailAppInAppGalleryAsync(string appId, string locale)
        {
            // Use HttpClientFactory to create an HttpClient instance
            using var _httpClient = _httpClientFactory.CreateClient();

            var InterfaceCode = await GetInterfaceCodeAsync();
            // Add headers to the HttpClient instance
            _httpClient.DefaultRequestHeaders.Add("Interface-Code", InterfaceCode);

            // Create a UriBuilder with the base URL
            var uriBuilder = new UriBuilder(BASE_URL + "/uowap/index");

            // Create a NameValueCollection with the query parameters
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["method"] = "internal.getTabDetail";
            query["serviceType"] = "20";
            query["reqPageNum"] = "1";
            query["maxResults"] = "25";
            query["uri"] = $"app|{appId}";
            query["accessId"] = "";
            query["appid"] = appId;
            query["zone"] = locale;
            query["locale"] = locale;

            // Assign the query string to the UriBuilder
            uriBuilder.Query = query.ToString();

            // Generate a cache key from the keyword and locale
            var cacheKey = $"{appId}_{locale}";

            // Check if the cache contains the key
            if (cache.TryGetValue(cacheKey, out string cachedContent))
            {
                // Return the cached content
                return cachedContent;
            }
            else
            {
                // Use async and await with using to dispose of the HttpResponseMessage object
                using (var response = await _httpClient.GetAsync(uriBuilder.Uri))
                {
                    // Throw an exception if the status code is not successful
                    response.EnsureSuccessStatusCode();

                    // Read and return the response content as a string
                    var content = await response.Content.ReadAsStringAsync();

                    // Set the cache options with a one day expiration time
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                    // Store the content in the cache with the key and options
                    cache.Set(cacheKey, content, cacheOptions);

                    // Return the content
                    return content;
                }
            }

        }

        public async Task<bool> CheckBaseUrlAsync()
        {
            var InterfaceCode = await GetInterfaceCodeAsync();
            var result = await NetworkUtils.IsLinkAvailableAsync($"{BASE_URL}/uowap/index?method=internal.getTabDetail&serviceType=20", InterfaceCode);
            return result;
        }

        public async Task<bool> CheckCloudUrlAsync()
        {
            var InterfaceCode = await GetInterfaceCodeAsync();
            var result = await NetworkUtils.IsLinkAvailableAsync(APPGALLERY_CLOUD_URL, InterfaceCode);
            return result;
        }
        public async Task<string> GetInterfaceCodeAsync()
        {
            // Use HttpClientFactory to create an HttpClient instance
            using var _httpClient = _httpClientFactory.CreateClient();

            // Create a UriBuilder with the base URL
            var uriBuilder = new UriBuilder($"{BASE_URL}/webedge/getInterfaceCode");

            // Create a NameValueCollection with the query parameters
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["locale"] = "en";

            // Assign the query string to the UriBuilder
            uriBuilder.Query = query.ToString();

            // Use async and await with using to dispose of the HttpResponseMessage object
            using var response = await _httpClient.GetAsync(uriBuilder.Uri);

            // Throw an exception if the status code is not successful
            response.EnsureSuccessStatusCode();

            // Read and return the response content as a string
            var result = await response.Content.ReadAsStringAsync();

            //fix the string
            result = result.Replace("\"", "");

            var timeStamps = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            return $"{result}_{timeStamps}";

        }
    }
}
