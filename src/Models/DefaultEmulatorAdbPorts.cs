using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiHMSInstaller.Models
{
    public static class DefaultEmulatorAdbPorts
    {
        public const int BLUESTACKS = 5555;
        public const int MEMUPLAYER = 21503;
        public const int NOXPLAYER = 62001;
        public const int MUMUPLAYER = 7555;


        public static List<int> GetAllDefaultEmulatorPorts()
        {
            var emulatorPorts = new List<int>();
            var fields = typeof(DefaultEmulatorAdbPorts).GetFields();
            foreach (var field in fields)
            {
                emulatorPorts.Add((int)field.GetValue(null));
            }
            return emulatorPorts;
        }
    }
}
