using DDNS.CloudFlare.Instance;
using DDNS.CloudFlare.Interface.DomainService;

namespace DDNS.CloudFlare.Interface.Callback
{
    public class URLCallback : ICallbacker
    {
        Config config;

        /// <summary>
        /// arsg:
        /// 0:ip
        /// 1:msg
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<string> CallAsync(params string[] args)
        {
            string url;
            if (args is null)
            {
                url = config.url;
            }
            else
            {
                url = config.url
                    .Replace("{ip}", args.Length >= 1 ? args[0] : "{ip}")
                    .Replace("{msg}", args.Length >= 2 ? args[1] : "{msg}");
            }

            using HttpClient httpClient = new();
            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri(url);
            var response = await httpClient.SendAsync(requestMessage);
            Logger.Debug("Calling " + requestMessage.RequestUri.ToString());
            var r = await response.Content.ReadAsStringAsync();
            Logger.Debug("Result: " + r);
            return r;
        }

        public URLCallback(Config cfg)
        {
            config = cfg;
        }

        public class Config
        {
            public string name = "call_url";
            public string url = "https://example.com";
        }
    }
}
