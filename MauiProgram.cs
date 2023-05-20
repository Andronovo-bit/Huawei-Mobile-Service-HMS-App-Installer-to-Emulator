using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Roboto-Medium.ttf", "Roboto-Medium");
                fonts.AddFont("Roboto-Regular.ttf", "Roboto-Regular");
                fonts.AddFont("FontAwesomeRegular.ttf", "FontRegular");  
                fonts.AddFont("FontAwesomeBrands.ttf", "FontBrands");  
                fonts.AddFont("FontAwesomeSolid.ttf", "FontSolid"); 
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

		return builder.Build();
	}
}