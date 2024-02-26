using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Integrations.Analytics;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.Services;

namespace HuaweiHMSInstaller.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly AnalyticsSubject _analyticsSubject;
        private readonly IAppGalleryService _appGalleryService;
        public SearchListItem SearchListItem { get; set; }
        public Worker<bool>.WorkCompletedEventHandler AfterEventInternetAndHuaweiServiceCheck { get; set; }


        public MainViewModel(
                INavigationService navigationService, 
                AnalyticsSubject analyticsSubject, 
                IAppGalleryService appGalleryService)
            : base(navigationService)
        {
            _appGalleryService = appGalleryService;
            _analyticsSubject = analyticsSubject;
        }

        public void Init(Action nonInternetAction)
        {
            _ = CheckInternetAndAppGalleryService(AfterEventInternetAndHuaweiServiceCheck, nonInternetAction);
        }

        //public async void NavigateToSettingsPage()
        //{
        //    await _navigationService.NavigateToAsync<SettingsPage>();
        //}

        public async void NavigateToDownloadAndInstallPage(SearchListItem listItem)
        {
           await NavigateToAsync<DownloadandInstallPage,DownloadAndInstallPageViewModel>(listItem);
        }

        public async Task CheckInternetAndAppGalleryService(Worker<bool>.WorkCompletedEventHandler func, Action nonInternetAction)
        {
            var networkState = CheckNetworkConnectionInit();
            var internetState = await NetworkUtils.CheckForInternetConnectionAsync();
            if (networkState && internetState)
            {
                CheckHuaweiService(func);
            }
            else
            {
                nonInternetAction?.Invoke();
                await _analyticsSubject.NotifyAsync("No Internet Connection");
            }
        }

        private bool CheckNetworkConnectionInit()
        {

            var current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet) return true;

            return false;
        }

        public void CheckHuaweiService(Worker<bool>.WorkCompletedEventHandler func) =>
                                CheckService(func, _appGalleryService.CheckAppGalleryServiceAsync);
        public void CheckHuaweiCloudService(Worker<bool>.WorkCompletedEventHandler func) =>
                                CheckService(func, _appGalleryService.CheckAppGalleryCloudServiceAsync);
        public void CheckService(Worker<bool>.WorkCompletedEventHandler func, Func<Task<bool>> check)
        {
            var worker = new Worker<bool>();
            worker.WorkCompleted += func;
            _ = worker.DoWorkAsync(async () => await check());
        }


    }
}
