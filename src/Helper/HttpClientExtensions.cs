using System.Diagnostics;

namespace HuaweiHMSInstaller.Helper
{
    public static class HttpClientExtensions
    {
        private static long contentLength = 0;
        // This extension method copies the source stream to the destination stream
        // and reports the progress to the IProgress<float> instance
        private static async Task CopyToAsync(this Stream source, Stream destination,
            IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            // Validate the arguments
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            // Use a constant buffer size for better performance
            const int bufferSize = 8192; //For adb
            var buffer = new byte[bufferSize];
            int bytesRead = 1;
            long totalRead = 0;

            // Read from the source stream and write to the destination stream
            // until the end of the source stream is reached
            bool canSeekAndReport = source.CanRead && progress != null; // check this condition only once
            float denominator = canSeekAndReport ? (contentLength > 0 ? contentLength : source.Length) : 0; // avoid division by zero
            
            while (bytesRead > 0)
            {
                bytesRead = await source.ReadAsync(buffer, 0, bufferSize, cancellationToken);
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                // Report the progress as a percentage of the source stream length or content length
                if (canSeekAndReport)
                {
                    var value = (float)totalRead / denominator;
                    if(value > 0.01)
                        progress.Report(value);
                }
            }
            return;

        }

        public static async Task DownloadAsync(this IHttpClientFactory clientFactory, string url, Stream file,
                IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            // Validate the arguments
            if (clientFactory == null) throw new ArgumentNullException(nameof(clientFactory));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (file == null) throw new ArgumentNullException(nameof(file));

            try
            {
                // Create an HttpClient instance using IHttpClientFactory
                var httpClient = clientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    // Send a GET request to the url and get the response stream
                    response.EnsureSuccessStatusCode();
                    // Get the content length from the response headers
                    contentLength = response.Content.Headers.ContentLength ?? 0;

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        // Copy the response stream to the file stream using the custom extension method
                        await responseStream.CopyToAsync(file, progress, cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException ex) // The request was canceled due to the timeout or the token
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        //public static async Task<long> GetFileSizeAsync(IHttpClientFactory clientFactory, string url)
        //{
        //    // Validate the arguments
        //    if (clientFactory == null) throw new ArgumentNullException(nameof(clientFactory));
        //    if (url == null) throw new ArgumentNullException(nameof(url));
        //    var httpClient = clientFactory.CreateClient();

        //    httpClient.BaseAddress = new Uri(url);

        //    // Send a GET request and read only the response headers
        //    var response = await httpClient.GetAsync("", HttpCompletionOption.ResponseHeadersRead);

        //    // Get the content-length value from the response
        //    var contentLength = response.Content.Headers.ContentLength; // returns a long? value

        //    return contentLength ?? 0;
        //}
        public static async Task<long> GetFileSizeAsync(IHttpClientFactory clientFactory, string url)
        {
            // Validate the arguments
            if (clientFactory == null) throw new ArgumentNullException(nameof(clientFactory));
            if (url == null) throw new ArgumentNullException(nameof(url));

            // Create a Uri instance from the url argument
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException($"The URL '{url}' is not a valid URI.", nameof(url));

            // Create an HttpClient instance using IHttpClientFactory
            var httpClient = clientFactory.CreateClient();
            try
            {
                // Send a GET request and read only the response headers
                var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                // Check for success
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"The request to '{url}' failed with status code {response.StatusCode}.");

                // Get the content-length value from the response
                if (!response.Content.Headers.TryGetValues("Content-Length", out var values) || string.IsNullOrEmpty(values.FirstOrDefault()))
                    throw new InvalidOperationException($"The URL '{url}' does not indicate a content length.");

                // Parse the content-length value as a long
                if (!long.TryParse(values.First(), out var contentLength))
                    throw new FormatException($"The content length value '{values.First()}' is not a valid long.");

                return contentLength;
            }
            catch(HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                throw ex;
            }

        }
    }
}
