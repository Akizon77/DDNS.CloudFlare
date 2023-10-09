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
    }
}