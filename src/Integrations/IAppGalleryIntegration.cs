namespace HuaweiHMSInstaller.Integrations
{
    public interface IAppGalleryIntegration
    {
        Task<string> SearchAppInAppGalleryAsync(string keyword, string locale);
        Task<string> AdvancedSearchAppInAppGalleryAsync(string keyword, string locale);
        Task<string> GetDetailAppInAppGalleryAsync(string appId, string locale);
        Task<bool> CheckBaseUrlAsync();
        Task<bool> CheckCloudUrlAsync();
    }
}
