using Newtonsoft.Json.Linq;
using System.Text;

namespace DDNS.CloudFlare
{
    public static class Helpers
    {
        static bool isConfigLoaded = false;
        static string Config;
        public static async Task<string> GetZoneID(string email, string APIkey, string domain)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri("https://api.cloudflare.com/client/v4/zones");
                request.Method = HttpMethod.Get;

                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("X-Auth-Email", email);
                request.Headers.Add("X-Auth-Key", APIkey);

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                try
                {
                    var json = JObject.Parse(result);
                    var zoneID = json["result"].First(v => v["name"].ToString() == domain)["id"];
                    return zoneID.ToString();
                }
                catch (Exception e)
                {
                    var j = JObject.Parse(result);
                    var errCode = j["errors"][0]["code"].ToString();
                    var errMsg = j["errors"][0]["message"].ToString();
                    if (errCode == "6003") Console.WriteLine("Email或者APIkey错误");
                }

            }
            catch (Exception e)
            {
                ShowErrMsg(e);
            }
            return "";

        }


        public static async Task<string> GetSecondLevelDomainID(string email, string APIkey,
            string zoneID, string secondLevevdomain)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{zoneID}/dns_records");
                request.Method = HttpMethod.Get;

                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("X-Auth-Email", email);
                request.Headers.Add("X-Auth-Key", APIkey);

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                var j = JObject.Parse(result);

                try
                {
                    var secondID = j["result"].First(v => v["name"].ToString() == secondLevevdomain)["id"];
                    return secondID.ToString();
                }
                catch (Exception e)
                {
                    ShowErrMsg(e);
                }
            }
            catch (Exception e)
            {
                ShowErrMsg(e);
            }
            return "";

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
            catch (Exception)
            {
                Console.WriteLine("无法通过API获取本机IP");
            }
            return "";
        }

        public static async Task ChangeIPAddress(string email, string apikey,
            string zoneID, string domainID, string ip, string domain, bool useIPv6)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri($"https://api.cloudflare.com/client/v4/zones/{zoneID}/dns_records/{domainID}/");
                request.Method = HttpMethod.Put;

                request.Headers.Add("Accept", "*/*");
                request.Headers.Add("X-Auth-Email", email);
                request.Headers.Add("X-Auth-Key", apikey);

                DomainInfo domainInfo;
                if (ip.Contains(':') && !useIPv6)
                {
                    throw new Exception("检测到IPv6,但是未在config.json启用IPv6，本次解析将失败");
                }
                if (ip.Contains(value: ':') && useIPv6)
                {
                    domainInfo = new DomainInfo() { content = ip, name = domain, type = "AAAA" };
                }
                else
                {
                    domainInfo = new DomainInfo() { content = ip, name = domain, type = "A" };
                }


                var bodyString = JObject.FromObject(domainInfo).ToString();

                request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();


                Console.WriteLine("\n" + result + "\n");

                try
                {
                    var success = JObject.Parse(result)["success"].ToString();
                    if (success == "true" || success == "True") Console.WriteLine($"成功解析 {domain} 到 {ip} ");
                }
                catch (Exception)
                {

                }
                try
                {
                    var code = JObject.Parse(result)["errors"][0]["code"].ToString();
                    var msg = JObject.Parse(result)["errors"][0]["message"].ToString();
                    Console.WriteLine($"尝试更改IP时发送错误，CloudFlare返回：{msg}");
                }
                catch (Exception)
                {

                }
            }
            catch (Exception e)
            {

                ShowErrMsg(e);
            }



        }

        static void ShowErrMsg(Exception e)
        {
            if (e.Message == "The SSL connection could not be established, see inner exception.")
            {
                Console.WriteLine("网络连接错误");
            }
            if (e.Message == "Sequence contains no matching element")
            {
                Console.WriteLine("无法找到域名(或二级域名)");
            }
            else
            {
                Console.WriteLine($"捕获到未知错误：{e.Message}\n 详细信息:{e.ToString}");
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
                Console.WriteLine("无法读取配置文件");
                Console.WriteLine(e);
            }
            return "";

        }
        class DomainInfo
        {
            public string type;
            public string name;
            public string content;
            public int ttl = 1;
            public bool proxied = false;
        }
    }
}
