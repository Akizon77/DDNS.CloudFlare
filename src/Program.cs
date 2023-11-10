using DDNS.CloudFlare;
using DDNS.CloudFlare.Instance;
using DDNS.CloudFlare.Interface.Callback;
using DDNS.CloudFlare.Interface.DomainService;
using DDNS.CloudFlare.Interface.IP;

var settings = Settings.GetInstance();
Logger.Info("开始读取配置文件");
var domainServices = Settings.DomainServices;
var callbacks = Settings.Callbackers;
List<DDNSer> dDNSers = new List<DDNSer>();
foreach (var service in domainServices)
{
    dDNSers.Add(new(service,new IPv4()));
}
List<Task> tasks = new List<Task>();
while (true)
{
    try
    {

        dDNSers.ForEach(service =>
        {
           //tasks.Add( service.Run());
           service.Run().GetAwaiter();
        });
        //Task.WhenAll(tasks).GetAwaiter();
        
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
        Logger.Error(e.ToString());
        Console.WriteLine("Will automatically restart in 30 seconds.");
        Thread.Sleep(new TimeSpan(0, 0, 30));
    }
}
class DDNSer
{
    IDomainService domainService;
    IObtainer obtainer;
    public DDNSer(IDomainService domainService, IObtainer obtainer)
    {
        this.domainService = domainService;
        this.obtainer = obtainer;
    }
    public async Task Run()
    {
        var ip = await obtainer.Get();
        await domainService.UpdateAsync(ip);
        await domainService.Callback(ip);
    }

}


