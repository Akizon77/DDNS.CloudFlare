namespace DDNS.CloudFlare.Interface.DomainService
{
    public interface IDomainService
    {
        public abstract Task<string> Callback(params string[] args);
        public string LastUpdateIP { get; }
        public bool IPChanged { get; }

        public bool Update(string ip)
        {
            return UpdateAsync(ip).GetAwaiter().GetResult();
        }

        abstract Task<bool> UpdateAsync(string ip);
        //TODO:Task<bool> Update(string ip)
        public string GetLastUpdataIP() => LastUpdateIP;

    }
}