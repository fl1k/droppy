using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Droppy_v1
{
    public class Configurator
    {
        public static BotConfig GetFromConfig = new BotConfig();
        static Configurator()
        {
            if (!File.Exists("data\\config.json"))
            {
                GetFromConfig = new BotConfig();
                string json = JsonConvert.SerializeObject(GetFromConfig, Formatting.Indented);
                File.WriteAllText("data\\config.json", json);
                Console.WriteLine("Config file created");
            }
            else
            {
                string json = File.ReadAllText("data\\config.json");
                GetFromConfig = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
    }

    public struct BotConfig
    {
        public string token;
        public string cmdPrefix;
        public ulong guildID;
        public string moderatorRole;
        public string dropperRole;
        public string dropRole;
        public string memberRole;
        public ulong botLogChannelID;
        public int queueMax;
        public double afkCheckTimeMinutes;
    }
}
