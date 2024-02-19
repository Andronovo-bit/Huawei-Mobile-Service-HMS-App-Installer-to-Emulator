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
                IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("Service provider is not initialized");

        public static T GetService<T>() where T : notnull
            => Current.GetRequiredService<T>();
    }
}
