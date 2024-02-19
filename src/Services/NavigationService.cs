using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Services
{
    public class NavigationService : INavigationService
    {
        public async Task NavigateToAsync<TPage>() where TPage : Page
        {
            try
            {
                await Shell.Current.GoToAsync(typeof(TPage).Name, true);
            } catch (Exception) {
                
                var pageName = typeof(TPage).Name;
                var route = $"///{pageName}";
                await Shell.Current.GoToAsync(route, true);
            }

        }

        public async Task NavigateToAsync<TPage, TViewModel>(params object[] args) 
                where TPage : Page
                where TViewModel : BaseViewModel
        {
            try { 
                var parameters = new Dictionary<string, object>();
                foreach (var arg in args)
                {
                    parameters.TryAdd(arg.GetType().Name, arg);
                }
              
               await Shell.Current.GoToAsync(typeof(TPage).Name, true, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task NavigateBackAsync()
        {
            await Shell.Current.GoToAsync("..", true);
        }

        public async Task NavigatePopToRootPage()
        {
            await Shell.Current.Navigation.PopToRootAsync(true);
        }

        public async Task NavigatePopToPage(bool animated)
        {
            await Shell.Current.Navigation.PopAsync(animated);
        }
    }
}
