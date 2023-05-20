using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Services
{
    static class ServiceProvider
    {
        static IServiceProvider Current =>
#if WINDOWS
			MauiWinUIApplication.Current.Services;
#elif ANDROID
                MauiApplication.Current.Services;
#elif IOS || MACCATALYST
			MauiUIApplicationDelegate.Current.Services;
#else
			throw new NotImplementedException();
#endif

        public static T GetService<T>() where T : notnull
            => Current.GetRequiredService<T>();
    }
}
