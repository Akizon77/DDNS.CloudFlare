using DDNS.CloudFlare;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

[DllImport("kernel32.dll")]
static extern bool SetConsoleCtrlHandler(ControlDelegate HandlerRoutine, bool Add);
ControlDelegate cancelHanlder = new(HandlerRoutine);
static bool HandlerRoutine(int CtrlType)
{
    Logger.Instance.WriteToFile("收到关闭指令");
    return false;
}
Logger logger;
try
{
    logger = new Logger();
}
catch (Exception)
{
    logger = new Logger(true);
    Console.WriteLine("日志文件被占用！是否开启了多个DDNS？");
}

logger.WriteToFile("载入日志模块完成");
SetConsoleCtrlHandler(cancelHanlder, true);
logger.WriteToFile($"载入关闭事件完成");
Logger.Instance.WriteToFile("单例运行测试通过");
logger.WriteToFile("开始读取配置文件");
var email =  Helpers.LoadConfig("email");
var apiKey = Helpers.LoadConfig("apiKey");
var domain = Helpers.LoadConfig("rootDomain");
var secondLevelDomain = Helpers.LoadConfig("ddnsDomain");
var useIPv6 = Convert.ToBoolean(Helpers.LoadConfig("useIPv6"));
var restartTime = Convert.ToInt32(Helpers.LoadConfig("restartTime"));
var autoRun = Convert.ToBoolean(Helpers.LoadConfig("autoRun"));
logger.WriteToFile($"读取配置文件成功！账号: {email} ,域名: {secondLevelDomain}");

while (true)
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

logger.WriteToFile("程序运行结束，退出");
