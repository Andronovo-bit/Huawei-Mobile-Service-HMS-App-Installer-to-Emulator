using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Models
{
    public record InstallApkModel(string Name, string DownloadUrl, string Description)
    {
        public bool IsDownloaded { get; set; }
        public InstallApkModel() : this(string.Empty, string.Empty, string.Empty) { }
    }
}
