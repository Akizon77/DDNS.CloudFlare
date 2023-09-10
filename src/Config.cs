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
        public Config()
        {
            Email = Helpers.LoadConfig("email");
            ApiKey = Helpers.LoadConfig("apiKey");
            RootDomain = Helpers.LoadConfig("rootDomain");
            Domain = Helpers.LoadConfig("ddnsDomain");
            useIPv6 = Convert.ToBoolean(Helpers.LoadConfig("useIPv6"));
            RestartTime = Convert.ToInt32(Helpers.LoadConfig("restartTime"));
            isAutoRestart = Convert.ToBoolean(Helpers.LoadConfig("autoRun"));
        }
    }
}
