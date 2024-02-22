using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Integrations.Analytics
{
    public interface IAnalyticsObserver
    {
        void UpdateAnalytics(string eventData);

        Task UpdateAnalyticsAsync(string eventData);
        
        Task UpdateAnalyticsAsync(string eventData, Dictionary<string, object> additionalData);

    }

}
