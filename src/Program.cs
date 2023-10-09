using DDNS.CloudFlare;
using DDNS.CloudFlare.Instance;
using DDNS.CloudFlare.Interface.Callback;
using DDNS.CloudFlare.Interface.DomainService;
using DDNS.CloudFlare.Interface.IP;

var settings = Settings.GetInstance();
Logger logger = InitLogger();
logger.Info("开始读取配置文件");
IDomainService service = (settings.domain_service) switch
{
    "cloudflare" => new Cloudflare(settings.cloudflare, new IPv4(), new URLCallback()),
    _ => throw new NotImplementedException("请检查配置文件，服务商目前仅支持Cloudflare，如需添加请向/Interface/DomainService添加源文件并编译")
};

while (true)
{
    try
    {
        Run(service);
        if (settings.auto_restart)
        {
            var timer = new TimerConut(settings.intervals);
            timer.Run();
        }
        else
            break;
    }
    catch (Exception e)
    {
        logger.Error(e.ToString());
        Console.WriteLine("Will automatically restart in 30 seconds.");
        Thread.Sleep(new TimeSpan(0, 0, 30));
    }
}
void Run(IDomainService domainService)
{
    domainService.Update();
}
Logger InitLogger()
{
    try
    {
        return new Logger();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Thread.Sleep(5000);
        throw;
    }
}