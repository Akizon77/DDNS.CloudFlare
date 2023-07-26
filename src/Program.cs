using DDNS.CloudFlare;
using Newtonsoft.Json.Linq;

var email =  Helpers.LoadConfig("email");
var apiKey = Helpers.LoadConfig("apiKey");
var domain = Helpers.LoadConfig("rootDomain");
var secondLevelDomain = Helpers.LoadConfig("ddnsDomain");
var useIPv6 = Convert.ToBoolean(Helpers.LoadConfig("useIPv6"));
var restartTime = Convert.ToInt32(Helpers.LoadConfig("restartTime"));
var autoRun = Convert.ToBoolean(Helpers.LoadConfig("autoRun"));
while(true)
{
    try
    {
        Console.WriteLine($"本次解析 {secondLevelDomain} 到本机地址");
        var ip = await Helpers.GetMyIPAddress(useIPv6);
        Console.WriteLine($"本机公网IP : {ip}");
        var zoneID = await Helpers.GetZoneID(email, apiKey, domain);
        var domainID = await Helpers.GetSecondLevelDomainID(email, apiKey, zoneID, secondLevelDomain);
        if (!String.IsNullOrEmpty(domainID)) Console.WriteLine($"找到域名 {secondLevelDomain} Token:{domainID}");
        await Helpers.ChangeIPAddress(email, apiKey, zoneID, domainID, ip, secondLevelDomain, useIPv6);
        if (autoRun)
        {
            var timer = new TimerConut(restartTime);
            timer.Run();
        }
        else break;
    }
    finally { }
}
