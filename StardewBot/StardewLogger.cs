using BlocklyBridge;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    class StardewLogger : ILogger
    {
        private IMonitor monitor;

        public StardewLogger(IMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void Log(object message)
        {
            monitor.Log($"{message}", LogLevel.Debug);
        }

        public void Warn(object message)
        {
            monitor.Log($"{message}", LogLevel.Warn);
        }
    }
}
