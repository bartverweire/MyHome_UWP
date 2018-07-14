using MetroLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MyHome
{
    public class Logger
    {
        private static ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<Logger>();

        public static void WriteLog(string tag, string message)
        {
            Log.Debug(tag + message);
        }
    }
}
