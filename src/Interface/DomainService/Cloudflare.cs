using DDNS.CloudFlare.Instance;
using DDNS.CloudFlare.Interface.Callback;
using DDNS.CloudFlare.Interface.IP;
using DDNS.CloudFlare.Network;
using Newtonsoft.Json.Linq;
using System.Text;

namespace DDNS.CloudFlare.Interface.DomainService
{
    public class Cloudflare : IDomainService
    {
        private readonly Config cfg;
        private Logger logger => Logger.Instance;

        private string rootDomain
        {
            get
            {
                var sp = cfg.Domain.Split('.');
                return string.Join(".", sp[sp.Length - 2], sp[sp.Length - 1]);
            }
        }

        public string LastUpdateIP
        {
            get => _lastupdateip;
            private set { _lastupdateip = value; }
        }

        private string ZoneID;
        private string DomainID;
        private string IP;
        private IObtainer ip_obtainer;
        private string _lastupdateip;
        private ICallbacker callbacker;

        public Cloudflare(Config cfg, IObtainer ip_obtainer, ICallbacker callbacker)
        {
            this.callbacker = callbacker;
            logger.Info($"Cloudflare配置已读取！账号: {cfg.Email} ,域名: {cfg.Domain}");
            this.cfg = cfg;
            this.ip_obtainer = ip_obtainer;
        }

        public async Task UpdateAsync()
        {
            IP = await ip_obtainer.Get();
            if (LastUpdateIP == IP)
            {
                logger.Warn($"检测到IP({IP})无变化，跳过解析");
                return;
            }
            logger.Debug($"Obtained the local IP: {IP}");
            await UpdateZoneID();
            if (!await UpdateDomainID())
                await CreateSubDomain();
            else
                await ChangeIPAddress();

            LastUpdateIP = IP;
            callbacker.Call(this);
        }

        public class Config
        {
            public string Email;
            public string ApiKey;
            public string Domain;

            //public bool Ipv6;

            public Config()
            { }

            public Config(string apikey, string email, string domain, bool ipv6)
            {
                this.Email = email;
                this.ApiKey = apikey;
                this.Domain = domain;
                //this.Ipv6 = ipv6;
            }
        }

        private async Task UpdateZoneID()
        {
            //存在ZoneID直接跳过
            if (!String.IsNullOrEmpty(ZoneID))
            {
                logger.Debug("ZoneID already exists, skipping");
                return;
            }
            logger.Debug("Requesting ZoneID");
            var result = await Get("https://api.cloudflare.com/client/v4/zones");
            // ↑ HTTP请求
            try
            {
                //尝试解析返回内容
                var json = JObject.Parse(result);
                logger.Debug($"Get root domain name by string manipulation：{rootDomain}");
                var zoneID = json["result"].First(v => v["name"].ToString() == rootDomain)["id"];
                //更新ZoneID
                ZoneID = zoneID.ToString();
                logger.Debug($"ZoneID: {ZoneID[0..6]}xxxxxxxxxxxx");
            }
            catch (Exception e)
            {
                //logger.Error(e.ToString());
                var j = JObject.Parse(result);
                logger.Error(j["errors"].ToString());
                var errCode = j["errors"][0]["code"].ToString();
                var errMsg = j["errors"][0]["message"].ToString();
                //↑ 解析错误代码
                if (errCode == "6003")
                {
                    throw new ArgumentException("Email或者APIkey错误");
                }
                //抛出 inner
                throw new HttpRequestException(
                    $"使用APIkey登录失败，Cloudflare返回:{errCode} \n" + errMsg,
                    e
                );
            }
        }

        //获取二级ID
        private async Task<bool> UpdateDomainID()
        {
            //存在DomainID直接跳过
            if (!String.IsNullOrEmpty(DomainID))
            {
                logger.Debug("DomainID already exists, skipping");
                return true;
            }
            logger.Debug("Requesting DomainID");
            try
            {
                var result = await Get(
                    $"https://api.cloudflare.com/client/v4/zones/{ZoneID}/dns_records"
                );
                var j = JObject.Parse(result);
                var secondID = j["result"].First(v => v["name"].ToString() == cfg.Domain)[
                    "id"
                ].ToString();
                if (String.IsNullOrEmpty(secondID))
                    throw new InvalidDataException("数据为空");
                DomainID = secondID;
                logger.Debug($"DomainID: {secondID[0..6]}xxxxxxxxxxxx");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task CreateSubDomain()
        {
            logger.Debug("Subdomain not exist, creating");
            DomainInfo domainInfo;
            if (IP.Contains(value: ':'))
            {
                domainInfo = new DomainInfo("AAAA", cfg.Domain, IP, false);
            }
            else
            {
                domainInfo = new DomainInfo("A", cfg.Domain, IP, false);
            }
            //写入请求内容
            var body = JObject.FromObject(domainInfo).ToString();
            var result = await Put(
                $"https://api.cloudflare.com/client/v4/zones/{ZoneID}/dns_records/{DomainID}/",
                body
            );
            var r = await Post(
                $"https://api.cloudflare.com/client/v4/zones/{ZoneID}/dns_records",
                body
            );
            var j = JObject.Parse(r);
            //logger.Debug(r);
            var success = j["success"].ToObject<Boolean>();
            if (success)
            {
                Logger.Instance.Info($"成功解析 {cfg.Domain} 到 {IP} ");
            }
            else
            {
                var code = JObject.Parse(result)["errors"][0]["code"].ToString();
                var msg = JObject.Parse(result)["errors"][0]["message"].ToString();
                throw new FormatException($"尝试更改IP时发送错误，CloudFlare返回：{msg}");
            }
            //if (String.IsNullOrEmpty(secondID))
            //    throw new InvalidDataException("数据为空");
            //DomainID = secondID;
        }

        private async Task ChangeIPAddress()
        {
            if (String.IsNullOrEmpty(ZoneID))
                throw new ArgumentException("ZoneID为空，请检查网络连接");
            if (String.IsNullOrEmpty(DomainID))
                throw new ArgumentException("DomainToken为空，请检查网络连接 或 先在CloudFlare里解析该二级域名");

            DomainInfo domainInfo;
            if (IP.Contains(value: ':'))
            {
                domainInfo = new DomainInfo("AAAA", cfg.Domain, IP, false);
            }
            else
            {
                domainInfo = new DomainInfo("A", cfg.Domain, IP, false);
            }
            //写入请求内容
            var body = JObject.FromObject(domainInfo).ToString();
            var result = await Put(
                $"https://api.cloudflare.com/client/v4/zones/{ZoneID}/dns_records/{DomainID}/",
                body
            );
            try
            {
                var success = JObject.Parse(result)["success"].ToString();
                if (success == "true" || success == "True")
                {
                    Logger.Instance.Info($"成功解析 {cfg.Domain} 到 {IP} ");
                }
            }
            catch (Exception)
            {
                var code = JObject.Parse(result)["errors"][0]["code"].ToString();
                var msg = JObject.Parse(result)["errors"][0]["message"].ToString();
                throw new FormatException($"尝试更改IP时发送错误，CloudFlare返回：{msg}");
            }
        }

        private async Task<string> Get(string url, string? body = null, HttpMethod? method = null)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = method ?? HttpMethod.Get;
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("X-Auth-Email", cfg.Email);
            request.Headers.Add("X-Auth-Key", cfg.ApiKey);
            if (body != null)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            return await EasyHttp.Get(request);
        }

        private async Task<string> Put(string url, string body)
        {
            return await Get(url, body, HttpMethod.Put);
        }

        private async Task<string> Post(string url, string body)
        {
            return await Get(url, body, HttpMethod.Post);
        }
    }

    internal class DomainInfo
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
            comment =
                $"Automatically updated by DDNS program at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss UTCz")}";
        }
    }
}