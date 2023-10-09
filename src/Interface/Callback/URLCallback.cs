using DDNS.CloudFlare.Instance;
using DDNS.CloudFlare.Interface.DomainService;

namespace DDNS.CloudFlare.Interface.Callback
{
    public class URLCallback : ICallbacker
    {
        public async Task<string> CallAsync(IDomainService domain)
        {
            using HttpClient httpClient = new();
            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri(
                Settings.GetInstance().callback_url.url.Replace("{ip}", domain.LastUpdateIP)
            );
            var response = await httpClient.SendAsync(requestMessage);
            Logger.Instance.Debug("Calling " + requestMessage.RequestUri.ToString());
            var r = await response.Content.ReadAsStringAsync();
            Logger.Instance.Debug("Result: " + r);
            return r;
        }

        public void Call(IDomainService domain)
        {
            CallAsync(domain).GetAwaiter().GetResult();
        }

        public URLCallback()
        {
        }

        public class Config
        {
            public string url;
        }
    }
}