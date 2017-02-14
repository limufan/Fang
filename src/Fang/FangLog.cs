using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fang
{
    public class FangLog
    {
        static FangLog()
        {
            _Config();
#if DEBUG
            Logger.Info("---------------DEBUG BUILD---------------------");
#else
            Logger.Info("---------------RELEASE BUILD---------------------");
#endif
        }

        public static ILog Logger { private set; get; }

        private static void _Config()
        {
            string log4netConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cacheLog4net.config");
            FileInfo configFileInfo = new FileInfo(log4netConfigFile);
            log4net.Config.XmlConfigurator.Configure(configFileInfo);
            Logger = log4net.LogManager.GetLogger("logger");
        }

        public static void Config()
        {
            _Config();

            FangLog.Logger.Error("Error");
            FangLog.Logger.Debug("Debug");
            FangLog.Logger.Fatal("Fatal");
            FangLog.Logger.Info("Info");
            FangLog.Logger.Warn("Warn");
        }
    }
}
