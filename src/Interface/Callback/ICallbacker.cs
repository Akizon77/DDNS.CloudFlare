using DDNS.CloudFlare.Interface.DomainService;

namespace DDNS.CloudFlare.Interface.Callback
{
    public interface ICallbacker
    {
        public string Call(params string[] args) => CallAsync(args).GetAwaiter().GetResult();
        public abstract Task<string> CallAsync(params string[] args);
        
    }
}