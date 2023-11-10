namespace DDNS.CloudFlare.Interface.DomainService
{
    public interface IDomainService
    {
        public string LastUpdateIP { get; }

        public void Update()
        {
            UpdateAsync().GetAwaiter().GetResult();
        }

        abstract Task UpdateAsync();
        //TODO:Task<bool> Update(string ip)
        //TODO:string GetLastUpdataIP()
        
    }
}