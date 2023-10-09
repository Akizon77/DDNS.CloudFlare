using DDNS.CloudFlare.Instance;
using DDNS.CloudFlare.Network;
using Newtonsoft.Json.Linq;

namespace DDNS.CloudFlare.Interface.IP
{
    public class IPv4 : IObtainer
    {
        private Logger logger => Logger.Instance;
        private List<string> ips = new();

        private readonly string[] interfaces =
        {
            "https://4.ipw.cn/",
            "https://ip.3322.net/",
            "https://ipv4.ddnspod.com/"
        };

        public async Task<string> Get()
        {
            foreach (var item in interfaces)
            {
                try
                {
                    var ip = await EasyHttp.Get(item);
                    ip = ip.Trim();
                    ip = ip.Replace("\n", "");
                    ip = ip.Replace("\r", "");
                    ip = ip.Replace("\t", "");
                    if (String.IsNullOrWhiteSpace(ip))
                        continue;
                    ips.Add(ip);
                    logger.Debug($"Get your IP:{ip} form {item}");
                }
                catch (Exception e)
                {
                    logger.Warn($"Unable to get local IP by {item}, because {e.Message}");
                }
            }
            try
            {
                var bilibiliip = await EasyHttp.Get("http://api.bilibili.com/x/web-interface/zone");
                var loliip = await EasyHttp.Get("https://net.lolicon.app/detail");
                var bj = JObject.Parse(bilibiliip)["data"]["addr"].ToString();
                ips.Add(bj);
                logger.Debug($"Get your IP:{bj} form http://api.bilibili.com/x/web-interface/zone");
                var lj = JObject.Parse(loliip)["ip"].ToString();
                ips.Add(lj);
                logger.Debug($"Get your IP:{lj} form https://net.lolicon.app/detail");
            }
            catch (Exception e)
            {
                logger.Warn($"Unable to get local IP by bilibili or cloudflare, because {e.Message}");
            }
            for (int i = 0; i < ips.Count; i++)
            {
                for (int j = i; j < ips.Count; j++)
                {
                    if (ips[i] == ips[j])
                        return ips[i];
                }
            }
            return ips.Count > 0 ? ips[0] : throw new HttpRequestException("无法通过API获取本机IP");
        }
    }
}