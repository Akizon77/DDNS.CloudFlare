﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDNS.CloudFlare
{
    public class DDNS
    {
        public static async Task DoDDNSLoop(bool isCancel = false,string thread = "Program")
        {

            Logger logger;
            try
            {
                logger = Logger.GetLogger();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(5000);
                throw;
            }

            logger.WriteToFile("开始读取配置文件", thread:thread);

            var config = Config.GetConfig();

            logger.WriteToFile($"读取配置文件成功！账号: {config.Email} ,域名: {config.Domain}",thread:thread);


            while (!isCancel)
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
                    await Network.ChangeIPAddress(config,thread);
                    //计时器
                    if (thread == "Program" && config.isAutoRestart)
                    {
                        var timer = new TimerConut(config.RestartTime);
                        timer.Run();
                    }
                    else break;
                }
                catch (Exception ex)
                {
                    logger.WriteToFile(ex.ToString(), "Error",thread);
                    Console.WriteLine(ex);
                    Thread.Sleep(5000);
                }
                Console.WriteLine();
            }
        }
    }
}