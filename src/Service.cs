using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DDNS.CloudFlare
{
    // 服务的使用
    // https://learn.microsoft.com/zh-cn/dotnet/core/extensions/windows-service?pivots=dotnet-6-0
    public class Service
    {
        static string ProcessPath => Environment.ProcessPath;
        static string ServiceName => "TravelAroundServerDDNSService";
        public static void Register()
        {
            
            //host.Run();

            ProcessStartInfo psi = new();
            psi.UseShellExecute = false;
            try
            {
                Remove();//删除之前的服务
                psi.FileName = $"sc";
                psi.Arguments = $"create \"{ServiceName}\" binpath= \"{ProcessPath}\" type= share start= auto displayname= \"TravelAroundServerDDNSService\"";
                Process.Start(psi);//新建服务
            }
            catch (Exception e)
            {
                Notifaction.SendText($"注册系统服务失败\n{e}");
            }
            Notifaction.SendText("成功注册系统服务\n将在开机时自动启动");
            Thread.Sleep(1000000);
        }
        public static void Remove()
        {
            try
            {
                ProcessStartInfo psi = new();
                psi.UseShellExecute = true;
                psi.FileName = $"sc";
                psi.Arguments = $"delete \"{ServiceName}\"";
                Process.Start(psi);//删除之前的服务
                Notifaction.SendText($"已卸载系统服务");
            }
            catch (Exception e)
            {
                Notifaction.SendText($"未能卸载服务\n{e}");
            }
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
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine(233);
            }
        }
    }
}
