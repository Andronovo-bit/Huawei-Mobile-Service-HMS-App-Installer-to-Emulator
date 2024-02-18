using HuaweiHMSInstaller.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync<TPage>() where TPage : Page;

        Task NavigateToAsync<TPage, TViewModel>(params object[] args) where TPage : Page where TViewModel : BaseViewModel;

        Task NavigateBackAsync();

        Task NavigatePopToRootPage();
    }
}

