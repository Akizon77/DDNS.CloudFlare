using DDNS.CloudFlare;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;

namespace DDNSProgram
{
    class Application
    {
        static string ProcessPath => Environment.ProcessPath;
        static string ServiceName = "TravelAroundServerDDNSService";
        static string CLIHelp = @"Welcome
环球旅行服务器DDNS程序，CLI使用指南：
-reg 注册系统服务，开机自启
-start 启动服务
-stop 停止服务
-rm 卸载系统服务，停止开机自启
-sr 以CLI服务模式运行";


        static int Main(string[] args)
        {
            #region 通过 IHost 创建服务
            IHostBuilder builder = Host.CreateDefaultBuilder();
            builder.UseWindowsService(x =>
            {
                x.ServiceName = ServiceName;
            });
            builder.ConfigureServices((content, service) =>
            {
                service.AddSingleton<Service>();
                service.AddHostedService<DDNSBackground>();
            });
            IHost host = builder.Build();
            #endregion
            #region 判断参数
            if (args.Length == 0)
            {
                Console.WriteLine(CLIHelp + "当前无参数，默认执行DDNS解析任务");
                DDNS.CloudFlare.DDNS.DoDDNSLoop().Wait();
            }
            else
            {
                args.ToList().ForEach(x =>
                {
                    if (x.Contains("-reg"))
                        Service.Register();
                    else if (x.Contains("-rm"))
                        Service.Remove();
                    else if (x.Contains("-start"))
                        Service.Start();
                    else if (x.Contains("-stop"))
                        Service.Stop();
                    else if (x.Contains("-sr"))
                        host.Run();
                    else
                        Console.WriteLine(CLIHelp + "\n输入的参数不正确");
                });
            }
            #endregion
            return 0;
        }

    }



}
