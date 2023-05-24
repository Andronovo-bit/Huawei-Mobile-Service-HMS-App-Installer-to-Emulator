using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // This extension method downloads the data from the url to the file stream
        // and reports the progress to the IProgress<float> instance
        public static async Task DownloadAsync(this HttpClient client, string url, Stream file,
            IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            // Validate the arguments
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (file == null) throw new ArgumentNullException(nameof(file));

            try
            {
                var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                // Send a GET request to the url and get the response stream
                using (response)
                {
                    response.EnsureSuccessStatusCode();
                    contentLength = response.Content.Headers.ContentLength ?? 0;
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        // Copy the response stream to the file stream using the custom extension method
                        await responseStream.CopyToAsync(file, progress, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }
    }
}
