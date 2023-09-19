using DDNS.CloudFlare;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

using System.Runtime.InteropServices;
namespace DDNSProgram
{
    class Application
    {
        static string ProcessPath;
        static string ServiceName = "TravelAroundServerDDNSService";
        static int Main(string[] args)
        {
            //初始化字段
            ProcessPath = Environment.ProcessPath;


            if (args.Length == 0)
            {
                Console.WriteLine(@"Welcome
环球旅行服务器DDNS程序，CLI使用指南：
-reg 注册系统服务，开机自启
-start 启动服务
-stop 停止服务
-rm 卸载系统服务，停止开机自启
当前没有CLI参数，默认执行DDNS解析");
                DoDDNSLoop().Wait();
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
                    else
                        Console.WriteLine("参数不正确！");
                });
            }
            
            return 0;
        }
        static async Task DoDDNSLoop()
        {

            Logger logger;
            try
            {
                logger = new Logger();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(5000);
                throw;
            }

            logger.WriteToFile("开始读取配置文件");

            var config = new Config();

            logger.WriteToFile($"读取配置文件成功！账号: {config.Email} ,域名: {config.Domain}");


            while (true)
            {
                try
                {
                    Console.WriteLine($"本次解析 {config.Domain} 到本机地址");
                    config.IP = await Network.GetMyIPAddress(config.useIPv6);
                    Console.WriteLine($"本机公网IP : {config.IP}");
                    try
                    {
                        config.ZoneID = await Network.GetZoneID(config);
                        config.DomainToken = await Network.GetSecondLevelDomainID(config);
                    }
                    catch (Exception ex)
                    {
                        //账号或API错误会直接throw，否则是网络暂时不佳导致的错误
                        //由于导致错误的原因已知（使用参数异常和网络异常区分），故CA2200警告可以忽略
                        //使用了新的变量ae，警告已消除
                        if (ex is ArgumentException ae) throw ae;
                        //此前获取到DomainToken 会忽略异常
                        if (!String.IsNullOrEmpty(config.DomainToken))
                        {
                            Console.WriteLine("捕获到可接受的异常");
                            Console.WriteLine(ex);
                            Console.WriteLine("由于ZoneID不会影响解析结果，故跳过此步骤");
                        }

                    }
                    await Network.ChangeIPAddress(config);
                    //计时器
                    if (config.isAutoRestart)
                    {
                        var timer = new TimerConut(config.RestartTime);
                        timer.Run();
                    }
                    else break;
                }
                catch (Exception ex)
                {
                    logger.WriteToFile(ex.ToString(), "Error");
                    Console.WriteLine(ex);
                }
                Console.WriteLine();
            }


            /* 运行逻辑：
             * 1.读取配置文件
             */

        }

        

        
    }
}