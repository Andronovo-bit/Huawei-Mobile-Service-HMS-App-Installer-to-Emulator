using Microsoft.Extensions.Caching.Memory;
using System.Web;

namespace HuaweiHMSInstaller.Integrations
{
    public class AppGalleryIntegration: IAppGalleryIntegration
    {
        // Define the parameters for the function call


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

            // Use UriBuilder and HttpUtility to construct the URL and query string
            var urlBuilder = new UriBuilder("https://web-dre.hispace.dbankcloud.cn/uowap/index");
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

            // Define the base URL and query parameters as constants
            const string url = "https://web-dre.hispace.dbankcloud.cn/uowap/index";
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
    }
}
