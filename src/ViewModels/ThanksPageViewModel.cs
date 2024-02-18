using HuaweiHMSInstaller.Services;

namespace HuaweiHMSInstaller.ViewModels
{
    public class ThanksPageViewModel : BaseViewModel
    {
        public ThanksPageViewModel(INavigationService navigationService) 
            : base(navigationService)
        {
        }

        //public async void NavigateToSettingsPage()
        //{
        //    await _navigationService.NavigateToAsync<SettingsPage>();
        //}

        public async Task NavigateToMainPage()
        {
            await _navigationService.NavigatePopToRootPage();
        }

    }
}
