using CommunityToolkit.Mvvm.ComponentModel;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Services;

namespace HuaweiHMSInstaller.ViewModels
{
    public abstract partial class BaseViewModel: ObservableObject
    {

        public readonly INavigationService _navigationService;
        public BaseViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public async Task NavigateToAsync<TPage>() where TPage : Page
        {
            await _navigationService.NavigateToAsync<TPage>();
        }

        public async Task NavigateToAsync<TPage, TViewModel>(params object[] args) 
            where TPage : Page 
            where TViewModel : BaseViewModel
        {
            await _navigationService.NavigateToAsync<TPage, TViewModel>(args);
        }

    }
}
