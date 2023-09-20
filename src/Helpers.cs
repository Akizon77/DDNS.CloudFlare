using Newtonsoft.Json.Linq;
using System.Text;

namespace DDNS.CloudFlare
{
    public delegate bool ControlDelegate(int CtrlType);
    public static class Helpers
    {

        static bool isConfigLoaded = false;
        static string Config;

        public static async Task Push()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://record.akz.moe/?cdata=c");
            request.Method = HttpMethod.Get;
            var resopnse = await client.SendAsync(request);
            resopnse.EnsureSuccessStatusCode();
        }
        /// <summary>
        /// 获取区域ID
        /// </summary>
        /// <param name="cfg">配置类</param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<string> GetZoneID(Config cfg)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://api.cloudflare.com/client/v4/zones");
            request.Method = HttpMethod.Get;

            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("X-Auth-Email", cfg.Email);
            request.Headers.Add("X-Auth-Key", cfg.ApiKey);

            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            // ↑ HTTP请求
            try
            {
                //尝试解析返回内容
                var json = JObject.Parse(result);
                var zoneID = json["result"].First(v => v["name"].ToString() == cfg.RootDomain)["id"];
                return zoneID.ToString();
            }
            catch (Exception e)
            {
                var j = JObject.Parse(result);
                var errCode = j["errors"][0]["code"].ToString();
                var errMsg = j["errors"][0]["message"].ToString();
                //↑ 解析错误代码
                if (errCode == "6003")
                {
                    throw new ArgumentException("Email或者APIkey错误");
                }
                //抛出 inner
                throw new HttpRequestException($"错误{errCode}", e);
            }

        }
        //获取二级ID
        public static async Task<string> GetSecondLevelDomainID(Config cfg)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{cfg.ZoneID}/dns_records");
                request.Method = HttpMethod.Get;
                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("X-Auth-Email", cfg.Email);
                request.Headers.Add("X-Auth-Key", cfg.ApiKey);
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                var j = JObject.Parse(result);
                var secondID = j["result"].First(v => v["name"].ToString() == cfg.Domain)["id"].ToString();
                if (String.IsNullOrEmpty(secondID)) throw new Exception();
                return secondID;

            }
            catch (Exception e)
            {
                throw new HttpRequestException("无法获取二级域名ID，请先在CloudFlare里解析该二级域名", e);
            }
        }

        public static async Task<string> GetMyIPAddress(bool acceptIPv6 = true)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();

                if (acceptIPv6)
                    request.RequestUri = new Uri("http://api.bilibili.com/x/web-interface/zone");
                else
                    request.RequestUri = new Uri("https://net.lolicon.app/detail");


                request.Method = HttpMethod.Get;

                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("User-Agent", "Thunder Client (https://www.thunderclient.com)");

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                var j = JObject.Parse(result);
                string ip;

                if (acceptIPv6)
                    ip = j["data"]["addr"].ToString();
                else
                    ip = j["ip"].ToString();
                return ip;
            }
            catch (Exception e)
            {
                throw new HttpRequestException("无法通过API获取本机IP", e);
            }
        }

        public static async Task ChangeIPAddress(Config config)
        {
            if (String.IsNullOrEmpty(config.ZoneID)) throw new ArgumentException("ZoneID为空，请检查网络连接");
            if (String.IsNullOrEmpty(config.DomainToken)) throw new ArgumentException("DomainToken为空，请检查网络连接 或 先在CloudFlare里解析该二级域名");
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{config.ZoneID}/dns_records/{config.DomainToken}/");
            request.Method = HttpMethod.Put;

            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("X-Auth-Email", config.Email);
            request.Headers.Add("X-Auth-Key", config.ApiKey);

            DomainInfo domainInfo;
            //判断是否是IPv6
            if (config.IP.Contains(':') && !config.useIPv6)
            {
                throw new NotSupportedException("获取到本机IPv6,但是未在config.json启用IPv6");
            }
            if (config.IP.Contains(value: ':') && config.useIPv6)
            {
                domainInfo = new DomainInfo("AAAA", config.Domain, config.IP, false);
            }
            else
            {
                domainInfo = new DomainInfo("A", config.Domain, config.IP, false);
            }

            //写入请求内容
            var bodyString = JObject.FromObject(domainInfo).ToString();

            request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            //Console.WriteLine(result);

            try
            {
                var success = JObject.Parse(result)["success"].ToString();
                if (success == "true" || success == "True")
                {
                    Logger.Instance.WriteToFile($"成功解析 {config.Domain} 到 {config.IP} ");
                    Console.WriteLine($"成功解析 {config.Domain} 到 {config.IP} ");
                }
                else throw new Exception();
            }
            catch (Exception)
            {
                var code = JObject.Parse(result)["errors"][0]["code"].ToString();
                var msg = JObject.Parse(result)["errors"][0]["message"].ToString();
                throw new FormatException($"尝试更改IP时发送错误，CloudFlare返回：{msg}");
            }
        }
        public static string LoadConfig(string filed)
        {
            try
            {
                if (!isConfigLoaded)
                {
                    Config = File.ReadAllText("config.json");
                    isConfigLoaded = true;
                }
                return JObject.Parse(Config)[filed].ToString();
            }
            catch (Exception e)
            {
                throw new FileLoadException("无法读取配置文件", e);
            }
        }
        class DomainInfo
        {
            public string type;
            public string name;
            public string content;
            public int ttl = 60;
            public bool proxied = false;
            public string comment;
            public DomainInfo(string type, string domain, string ip, bool proxied = false)
            {
                this.type = type;
                this.name = domain;
                this.proxied = proxied;
                this.content = ip;
                comment = $"Automatically updated by DDNS program at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss UTCz")}";
            }
        }
    }
    public class Logger
    {
        readonly string LogPath = "logs";
        readonly string LatestLog = "logs/latest.log";
        public static Logger? Instance;
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
                    throw new FileLoadException("Log is occuiped by other program.日志文件正在被其他程序占用",e);
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
        public void WriteToFile(string msg, string warnLevel = "Info")
        {
            try
            {
                sw.WriteLine($"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]")}[{warnLevel}]{msg}");
                sw.Flush();
            }
            catch (Exception)
            {
                throw new FileLoadException("Log is occuiped by other program.日志文件正在被其他程序占用");
            }

        }
    }
}
