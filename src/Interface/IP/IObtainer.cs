namespace DDNS.CloudFlare.Interface.IP
{
    public interface IObtainer
    {
        abstract Task<string> Get();
    }
}