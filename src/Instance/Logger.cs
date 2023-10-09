namespace DDNS.CloudFlare.Instance
{
    public class Logger
    {
        private readonly string LogPath = "logs";
        private readonly string LatestLog = "logs/latest.log";
        public static Logger? Instance;
        private StreamWriter? sw;

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
                    string newFilePath =
                        $"{LogPath}/{fileInfo.LastWriteTime.ToString("yyyy-MM-dd")}";
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
                    throw new FileLoadException(
                        "Log is occuiped by other program.日志文件正在被其他程序占用",
                        e
                    );
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

        [Obsolete("Use Info(),Warn(),Error() or Debug() instead")]
        public void WriteToFile(string msg, string warnLevel = "Info")
        {
            try
            {
                Console.ResetColor();
                Console.Write(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"));
                switch (warnLevel)
                {
                    case "Info":
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;

                    case "Warn":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case "Error":
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case "Debug":
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                }
                Console.Write($"[{warnLevel}]");
                Console.ResetColor();
                Console.WriteLine(msg);
                sw.WriteLine($"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")}[{warnLevel}]{msg}");
                sw.Flush();
            }
            catch (Exception)
            {
                throw new FileLoadException("Log is occuiped by other program.日志文件正在被其他程序占用");
            }
        }

        public void Error(string msg)
        {
            WriteToFile(msg, "Error");
        }

        public void Info(string msg)
        {
            WriteToFile(msg, "Info");
        }

        public void Warn(string msg)
        {
            WriteToFile(msg, "Warn");
        }

        public void Debug(string msg)
        {
            if (Settings.Instance.debug)
                WriteToFile(msg, "Debug");
        }
    }
}