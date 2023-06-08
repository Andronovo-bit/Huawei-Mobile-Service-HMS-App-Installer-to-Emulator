using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Integrations
{
    public interface IAppGalleryIntegration
    {
        Task<string> SearchAppInAppGalleryAsync(string keyword, string locale);
        Task<string> AdvancedSearchAppInAppGalleryAsync(string keyword, string locale);
        Task<string> GetDetailAppInAppGalleryAsync(string appId, string locale);
    }
}
