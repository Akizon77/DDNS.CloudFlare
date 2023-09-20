using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DDNS.CloudFlare
{
    // 服务的使用
    // https://learn.microsoft.com/zh-cn/dotnet/core/extensions/windows-service?pivots=dotnet-6-0
    public class Service
    {
        static string ProcessPath => Environment.ProcessPath;
        static string ServiceName => "TravelAroundServerDDNSService";
        static string Description => "DDNS小程序";
        public static void Register()
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = false;
            Remove();//删除之前的服务
            psi.FileName = $"sc";
            psi.Arguments = $"create {ServiceName} binpath= \"{ProcessPath} -sr\" type= share start= auto displayname= \"TravelAroundServerDDNSService\"";
            Process.Start(psi);//新建服务
            psi.Arguments = $"description {ServiceName} \"{Description}\"";
            Process.Start(psi);

        }
        public static void Remove()
        {

            ProcessStartInfo psi = new();
            psi.UseShellExecute = false;
            psi.FileName = $"sc";
            psi.Arguments = $"delete {ServiceName}";
            Process.Start(psi);//删除之前的服务
        }
        public static void Start()
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = true;
            psi.FileName = $"cmd.exe";
            psi.Arguments = $"/c net start {ServiceName}";
            Process.Start(psi);

        }
        public static void Stop()
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = true;
            psi.FileName = $"cmd.exe";
            psi.Arguments = $"/c net stop {ServiceName}";
            Process.Start(psi);
        }
        static bool TrySth(Action doSth)
        {
            try
            {
                doSth();
            }
            catch { return false; }
            return true;
        }


    }

    public class DDNSBackground : BackgroundService
    {
        Logger logger => Logger.GetLogger();
        const string thread = "Service";
        public readonly ILogger<DDNSBackground> _logger;
        public DDNSBackground(ILogger<DDNSBackground> l)
        {
            _logger = l;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("服务启动");
            logger.WriteToFile("服务启动",thread:thread);
            //Task.Run( ()=> ExecuteAsync(cancellationToken));
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("服务终止");
            logger.WriteToFile("服务终止", thread: thread);
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("服务运行中");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("服务运行中");
                //logger.WriteToFile("服务运行中","Info",thread);
                DDNS.DoDDNSLoop(stoppingToken.IsCancellationRequested,thread);
                await Task.Delay(600*1000);
            }
        }
        
    }
}
