using BlocklyBridge;
using CsvHelper;
using CsvHelper.Configuration;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    public class StardewLogger : ILogger
    {
        private IMonitor monitor;
        private String path;


        public StardewLogger(IMonitor monitor)
        {
            this.monitor = monitor;

            String DirectoryPath = Path.Combine(ModEntry.helper.DirectoryPath, "Log");

            Directory.CreateDirectory(DirectoryPath);

            path = Path.Combine(DirectoryPath, "Log.csv");
        }

        public void LogSessionStart()
        {
            Log("", "", justNewLine: true);
            Log("Start Game Session", "Game.Start", header: true);
        }

        public void Log(object message)
        {
            Log(message, null, null);
        }

        public void Log(object message, String action = null, String data = null, bool justNewLine = false, bool header = false)
        {

            if (action is not null)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    // Don't write the header again.
                    HasHeaderRecord = header,
                };

                var date = justNewLine ? "" : DateTime.Now.ToString();
                var records = new List<LogItem> {
                    new LogItem {Date = date, Action = action, Message = $"{message}", Data = data ?? "" },
                };
                
                monitor.Log($"{message}", LogLevel.Debug);
                using (var sw = new StreamWriter(path, true))
                using (CsvWriter csv = new CsvWriter(sw, config))
                {
                    csv.WriteRecords(records);
                }
            }
        }

        public void Warn(object message)
        {
            monitor.Log($"{message}", LogLevel.Warn);
        }
    }

    public class LogItem
    {
        public String Date { get; set; }
        public String Action { get; set; }
        public String Message { get; set; }
        public String Data { get; set; }
    }

}
