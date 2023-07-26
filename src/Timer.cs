namespace DDNS.CloudFlare
{
    public  class TimerConut
    {
        public int Time;
        public  void Run()
        {
            int countDownSeconds = 10;
            Console.WriteLine($"还有 {Time} 秒钟重新运行");
            var timer = new Timer(TimerCallback, countDownSeconds, 0, 1000);
            Thread.Sleep(1000 * Time);
            timer.Dispose();  // 停止计时器
        }

        public TimerConut(int seconds) 
        { 
            Time = seconds;
        }
         void TimerCallback(object state)
        {
            try
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);  //将光标移动到行的开头
                Console.Write(new string(' ', Console.WindowWidth));  // 覆盖之前的打印内容
                Console.SetCursorPosition(0, Console.CursorTop);  //将光标移动到行的开头
                Console.WriteLine($"还有 {Time} 秒钟重新运行");
            }
            finally
            {
                Time--;
            }
        }
    }



}

