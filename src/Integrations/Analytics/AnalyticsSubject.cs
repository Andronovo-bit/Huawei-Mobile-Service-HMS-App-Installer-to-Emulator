namespace HuaweiHMSInstaller.Integrations.Analytics
{
    public class AnalyticsSubject
    {
        private List<IAnalyticsObserver> _observers = new();

        public void Attach(IAnalyticsObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IAnalyticsObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(string eventData)
        {
            foreach (var observer in _observers)
            {
                observer.UpdateAnalytics(eventData);
            }
        }

        // Modify the Notify method to be asynchronous
        public async Task NotifyAsync(string eventData)
        {
            var tasks = _observers.Select(observer => observer.UpdateAnalyticsAsync(eventData));
            await Task.WhenAll(tasks);
        }

        public async Task NotifyAsync(string eventData, Dictionary<string, object> additionalData)
        {
            var tasks = _observers.Select(observer => observer.UpdateAnalyticsAsync(eventData, additionalData));
            await Task.WhenAll(tasks);
        }
    }

}
