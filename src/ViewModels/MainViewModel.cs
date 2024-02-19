using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.Services;

namespace HuaweiHMSInstaller.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public SearchListItem SearchListItem { get; set; }

        public MainViewModel(INavigationService navigationService) 
            : base(navigationService)
        {
        }

        //public async void NavigateToSettingsPage()
        //{
        //    await _navigationService.NavigateToAsync<SettingsPage>();
        //}

        public async void NavigateToDownloadAndInstallPage(SearchListItem listItem)
        {
           await NavigateToAsync<DownloadandInstallPage,DownloadAndInstallPageViewModel>(listItem);
        }

    }
}
