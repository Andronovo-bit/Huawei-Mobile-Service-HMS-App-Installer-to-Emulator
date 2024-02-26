using HuaweiHMSInstaller.Models.MappingModels;

namespace HuaweiHMSInstaller.Services
{
    public interface IAppGalleryService
    {
        Task<AppGalleryAdvancedSearchResult> SearchAppGalleryApp(string keyword);
        Task<AppGalleryAppDetailResult> GetAppDetail(string appId);
        Task<bool> CheckAppGalleryServiceAsync();
        Task<bool> CheckAppGalleryCloudServiceAsync();
    }
}
