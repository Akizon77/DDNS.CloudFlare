using Newtonsoft.Json.Linq;

namespace DDNS.CloudFlare.Network
{
    public class EasyHttp
    {
        private static HttpClient client = new();

        public static async Task<string> Get(string url)
        {
            return await Get(new HttpRequestMessage() { RequestUri = new Uri(url) });
        }

        public static async Task<string> Get(HttpRequestMessage request)
        {
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class IP
    {
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
    }
}