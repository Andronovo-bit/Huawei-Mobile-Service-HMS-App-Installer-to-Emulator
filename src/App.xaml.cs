using HuaweiHMSInstaller.Pages;
using System.Reflection;

namespace HuaweiHMSInstaller
{
    public partial class App : Application
    {
        private static Mutex mutex = new(true, Assembly.GetEntryAssembly().GetName().Name);

        public App()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Current.Quit();
                Environment.Exit(0);
            }

            InitializeComponent();

            MainPage = new AppShell();
            Routing.RegisterRoute(nameof(DownloadandInstallPage), typeof(DownloadandInstallPage));
            Routing.RegisterRoute(nameof(ThanksPage), typeof(ThanksPage));
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            window.MinimumWidth = 600;
            window.MinimumHeight = 600;
            return window;
        }
    }
}