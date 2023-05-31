using CommunityToolkit.Maui;
using HuaweiHMSInstaller.Integrations;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Resources.Languages;
using HuaweiHMSInstaller.Services;
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
			.UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .UseLocalizationResourceManager(settings =>
            {
                settings.RestoreLatestCulture(true);
                settings.AddResource(AppResources.ResourceManager);
                settings.InitialCulture(new System.Globalization.CultureInfo("en-Us"));
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                fonts.AddFont("FA-Brands-Regular-400.otf", "FontBrands");  
                fonts.AddFont("FA-Free-Regular-400.otf", "FontRegular");  
                fonts.AddFont("FA-Free-Solid-900.otf", "FontSolid"); 
            });

        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {

#if WINDOWS
                var size = new Windows.Graphics.SizeInt32();
                size.Height = 720;
                size.Width  = 1150;
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                var corew = nativeWindow.CoreWindow;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                var newPositionX = (int)(((appWindow.Size.Width * appWindow.Position.X ) / size.Width) +  (appWindow.Position.X));
                var newPositionY = (int)((appWindow.Size.Height * appWindow.Position.Y ) / size.Height);
                appWindow.Resize(size);
                // find center of screen
                var appWindowPosition =  appWindow.Position;                
                appWindowPosition.X = newPositionX;
                appWindowPosition.Y = newPositionY;
                appWindow.Move(appWindowPosition);
                
#endif

        });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        var services = builder.Services;
        builder.Services.Configure<GlobalOptions>(x => x.ProjectOperationPath = Path.Combine(Path.GetTempPath(),"HuaweiHMSInstaller")); //configure value

        services.AddHttpClient();
        services.AddScoped<IAdbOperationService, AdbOperationService>();
        services.AddScoped<IAppGalleryIntegration, AppGalleryIntegration>();
        services.AddScoped<IAppGalleryService, AppGalleryService>();

        return builder.Build();
	}
}