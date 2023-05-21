using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Helper
{
    public static class CheckInternetConnection
    {
        public static bool IsConnected()
        {
            try
            {
                System.Net.IPHostEntry i = System.Net.Dns.GetHostEntry("www.google.com");
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
            var client = new HttpClient();
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


    }
}
