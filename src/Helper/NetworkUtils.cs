using System.Net;

namespace HuaweiHMSInstaller.Helper
{
    public static class NetworkUtils
    {
        public static bool IsConnected()
        {
            try
            {
                IPHostEntry i = Dns.GetHostEntry("www.google.com");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> CheckForInternetConnectionAsync(int timeoutMs = 10000)
        {
            const string url = "http://www.gstatic.com/generate_204";
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            try
            {
                var response = await client.GetAsync(url);
                return response.StatusCode == HttpStatusCode.NoContent;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.RequestTimeout)
            {
                // Handle request timeout exception
                Console.WriteLine("HttpRequestException: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Handle any other exception
                Console.WriteLine("Exception: " + ex.Message);
                return false;
            }
        }
        public static async Task<bool> IsLinkAvailableAsync(string url, string interfaceCode = null)
        {
            // Use a using declaration to dispose the HttpClient instance.
            using HttpClient client = new();

            // Add headers to the HttpClient instance
            client.DefaultRequestHeaders.Add("Interface-Code", interfaceCode);

            // Set the Timeout property to 10 second.
            client.Timeout = TimeSpan.FromSeconds(10);

            // Try to get the response from the link using a try-catch block.
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                // Check if the response is successful.
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Return false if any exception occurs.
                return false;
            }
        }
    }
}
