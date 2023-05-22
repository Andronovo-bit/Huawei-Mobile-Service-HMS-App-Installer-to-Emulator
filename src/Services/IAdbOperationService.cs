using AdvancedSharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Services
{
    public interface IAdbOperationService
    {
        public bool CheckAdbServer();
        public Task DownloadAdbFromInternet();
        public Task<List<DeviceData>> GetDevices();
    }
}
