using DDNS.CloudFlare.Interface.DomainService;

namespace DDNS.CloudFlare.Interface.Callback
{
    public interface ICallbacker
    {
        public abstract void Call(IDomainService domainService);
        
    }
}