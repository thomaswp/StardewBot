using BlocklyBridge;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewBot
{
    public class StardewLogger
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
            bool Exists = File.Exists(path);
            var message = new JObject();
            message.Add("Game Session", "Start");

            if (Exists)
            {
                Log(new JObject(), action: "", justNewLine: true);

                Log(message, "Game.Start");
            } else
            {
                Log(message, "Game.Start", header: true);
            }   
        }

        public void Log(object message)
        {
            Log(message, null);
        }

        public void Log(object msg, String action = null, String code = null, bool justNewLine = false, bool header = false, bool displayMessage = true)
        {
            if(displayMessage) monitor.Log($"{msg}", LogLevel.Debug);

            var methodInfo = new StackTrace().GetFrame(3).GetMethod();
            if(methodInfo.Name.Equals("SaveCode"))
            {
                action = "IDE.CodeChanged";
            }

            if (action is not null)
            {
                String message;

                if(msg is JObject)
                {
                    message = ((JObject)msg).ToString();
                } else
                {
                    message = $"{msg}";
                }

                //JObject JMessage = (JObject) message;

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    // Don't write the header again.
                    HasHeaderRecord = header,
                };
                String gametime = "";
                String gamemoney = "";

                if (Context.IsWorldReady)
                {
                    if (Game1.Date != null)
                    {
                        gametime = Game1.Date.ToString() + Game1.timeOfDay.ToString();
                    }

                    if (Game1.player != null)
                    {
                        gamemoney = Game1.player.Money.ToString();
                    }
                }


                var date = justNewLine ? "" : DateTime.Now.ToString();
                var records = new List<LogItem> {
                    new LogItem {Date = date, Action = action, Message = message, Code = code, InGameTime = gametime, InGameMoney = gamemoney },
                };

                using (var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(stream))
                using (CsvWriter csv = new CsvWriter(writer, config))
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
        public String Code { get; set; }
        public String InGameTime { get; set; }
        public String InGameMoney { get; set; }
    }

}
