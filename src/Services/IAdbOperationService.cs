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
        public IProgress<float> Progress { get;set; }
        public bool CheckAdbServer();
        public Task DownloadAdbFromInternetAsync(IProgress<float> progress = null);
        public Task<List<DeviceData>> GetDevices();
    }
}
