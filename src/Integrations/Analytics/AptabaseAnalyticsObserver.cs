using Aptabase.Maui;

namespace HuaweiHMSInstaller.Integrations.Analytics
{
    /// <summary>
    /// Observes and updates analytics using the Aptabase client.
    /// </summary>
    public class AptabaseAnalyticsObserver : IAnalyticsObserver
    {
        private readonly IAptabaseClient _aptabase;

        public AptabaseAnalyticsObserver(IAptabaseClient aptabase)
        {
            _aptabase = aptabase;
        }

        /// <summary>
        /// Updates analytics with the given event name.
        /// </summary>
        public void UpdateAnalytics(string eventName)
        {
            _aptabase.TrackEvent(eventName);
        }

        /// <summary>
        /// Asynchronously updates analytics with the given event name.
        /// </summary>
        public async Task UpdateAnalyticsAsync(string eventName)
        {
           await Task.Run(() => _aptabase.TrackEvent(eventName)).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously updates analytics with the given event name and additional data.
        /// </summary>
        public async Task UpdateAnalyticsAsync(string eventName, Dictionary<string, object> additionalData)
        {
           await Task.Run(() => _aptabase.TrackEvent(eventName, additionalData)).ConfigureAwait(false);
        }
    }
}