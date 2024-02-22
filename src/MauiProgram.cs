using System.Reflection;
using CommunityToolkit.Maui;
using HuaweiHMSInstaller.Integrations;
using HuaweiHMSInstaller.Integrations.Analytics;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.Resources.Languages;
using HuaweiHMSInstaller.Services;
using HuaweiHMSInstaller.ViewModels;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace HuaweiHMSInstaller;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // .UseAptabase("YOUR-CODE") // 👈 this is where you enter your App Key
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .UseLocalizationResourceManager(settings =>
            {
                settings.RestoreLatestCulture(true);
                settings.AddResource(AppResources.ResourceManager);
            })
            .ConfigureFonts(ConfigureFonts);

        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {

            //#if WINDOWS
            //                var size = new Windows.Graphics.SizeInt32();
            //                size.Height = 720;
            //                size.Width  = 1150;
            //                var mauiWindow = handler.VirtualView;
            //                var nativeWindow = handler.PlatformView;
            //                var corew = nativeWindow.CoreWindow;
            //                nativeWindow.Activate();
            //                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            //                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            //                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            //                var newPositionX = (int)(((appWindow.Size.Width * appWindow.Position.X ) / size.Width) +  (appWindow.Position.X));
            //                var newPositionY = (int)((appWindow.Size.Height * appWindow.Position.Y ) / size.Height);
            //                appWindow.Resize(size);
            //                // find center of screen
            //                var appWindowPosition =  appWindow.Position;                
            //                appWindowPosition.X = newPositionX;
            //                appWindowPosition.Y = newPositionY;
            //                appWindow.Move(appWindowPosition);

            //#endif

        });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.ConfigureServices();
        // builder.Services.RegisterAnalyticsObservers(); // 👈 this is where we register the observers
        builder.RegisterViews();

        var app = builder.Build();
        // app.Services.ConfigureAnalyticsSubject(); // 👈 this is where we configure the subject
        return app;
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingletonWithShellRoute<MainPage, MainViewModel>(nameof(MainPage));
        builder.Services.AddSingletonWithShellRoute<DownloadandInstallPage, DownloadAndInstallPageViewModel>(nameof(DownloadandInstallPage));
        builder.Services.AddSingletonWithShellRoute<ThanksPage, ThanksPageViewModel>(nameof(ThanksPage));

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<DownloadAndInstallPageViewModel>();
        builder.Services.AddTransient<ThanksPageViewModel>();


        return builder;
    }
    private static void ConfigureServices(this IServiceCollection services)
    {
        services.Configure<GlobalOptions>(x => x.ProjectOperationPath = Path.Combine(Path.GetTempPath(), "HuaweiHMSInstaller")); //configure value
        services.Configure<GlobalOptions>(x => x.VersionNumber = "0.0.1"); //configure value
        services.AddHttpClient();
        services.AddScoped<IAdbOperationService, AdbOperationService>();
        services.AddScoped<IAppGalleryIntegration, AppGalleryIntegration>();
        services.AddScoped<IAppGalleryService, AppGalleryService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<AnalyticsSubject>();
    }
    private static void ConfigureAnalyticsSubject(this IServiceProvider serviceProvider)
    {
        // Use serviceProvider to get AnalyticsSubject and observers, then configure
        var subject = serviceProvider.GetRequiredService<AnalyticsSubject>();
        var observers = serviceProvider.GetServices<IAnalyticsObserver>();
        foreach (var observer in observers)
        {
            subject.Attach(observer);
        }
    }
    private static void ConfigureFonts(IFontCollection fonts)
    {
        // Centralizing font configuration allows for easier modifications and extensions
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
        fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
        fonts.AddFont("FA-Brands-Regular-400.otf", "FontBrands");
        fonts.AddFont("FA-Free-Regular-400.otf", "FontRegular");
        fonts.AddFont("FA-Free-Solid-900.otf", "FontSolid");
    }
    private static void RegisterAnalyticsObservers(this IServiceCollection services)
    {
        // Use reflection to find all types that implement IAnalyticsObserver
        // This allows adding new observers without modifying this code
        var observerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(IAnalyticsObserver).IsAssignableFrom(t) && !t.IsInterface);

        foreach (var type in observerTypes)
        {
            services.AddSingleton(typeof(IAnalyticsObserver), type);
        }
    }
}