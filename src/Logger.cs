using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDNS.CloudFlare
{
    public class Logger
    {
        readonly string LogPath = "logs";
        readonly string LatestLog = "logs/latest.log";
        public static Logger? Instance;
        public static Logger GetLogger() => Instance ?? new Logger();
        StreamWriter? sw;
        public Logger()
        {
            Directory.CreateDirectory(LogPath);
            if (File.Exists(LatestLog))
            //检查是否存在已有的配置文件
            {
                try
                {
                    int repeatTimes = 1;
                    FileInfo fileInfo = new FileInfo(LatestLog);
                    string newFilePath = $"{LogPath}/{fileInfo.LastWriteTime.ToString("yyyy-MM-dd")}";
                    while (true)
                    {
                        if (!File.Exists(newFilePath + $"-{repeatTimes}.log"))
                        {
                            File.Move(LatestLog, newFilePath + $"-{repeatTimes}.log");
                            break;
                        }
                        repeatTimes++;
                    }
                }
                catch (Exception e)
                {
                    throw new FileLoadException("Log is occuiped by other program.日志文件正在被其他程序占用", e);
                }

            }
            //必须单个实例，否则文件会被多个实例占用导致无法写入
            sw = File.CreateText(LatestLog);
            Instance = this;
        }
        public Logger(bool occuiped)
        {
            Instance = this;
        }
        public void WriteToFile(string msg,string warnLevel = "Info",string thread = "Program")
        {
            try
            {
                sw.WriteLine($"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")}[{thread}/{warnLevel}]{msg}");
                sw.Flush();
            }
            catch (Exception)
            {
                throw new FileLoadException("Log is occuiped by other program.日志文件正在被其他程序占用");
            }

        }
        
    }
}
