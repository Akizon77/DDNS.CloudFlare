using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDNS.CloudFlare
{
    public class Config
    {
        public string Email;
        public string ApiKey;
        public string RootDomain;
        public string Domain;
        public string DomainToken;
        public string ZoneID;
        public string IP;
        public bool useIPv6;
        public bool isAutoRestart;
        public int RestartTime;
        private string jsonContent;

        public static Config GetConfig() => new Config();

        public Config()
        {
            jsonContent = File.ReadAllText("config.json");
            Email = LoadConfig("email");
            ApiKey = LoadConfig("apiKey");
            RootDomain = LoadConfig("rootDomain");
            Domain = LoadConfig("ddnsDomain");
            useIPv6 = bool.Parse(LoadConfig("useIPv6"));
            RestartTime = int.Parse(LoadConfig("restartTime"));
            isAutoRestart = bool.Parse(LoadConfig("autoRun"));
        }

        public string LoadConfig(string filed)
        {
            try
            {
                return JObject.Parse(jsonContent)[filed].ToString();
            }
            catch (Exception e)
            {
                throw new FileLoadException("无法读取配置文件", e);
            }
        }
    }
}
