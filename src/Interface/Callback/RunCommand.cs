using DDNS.CloudFlare.Interface.DomainService;
using System.Diagnostics;

namespace DDNS.CloudFlare.Interface.Callback
{
    public class RunCommand : ICallbacker
    {
        private readonly Config config;

        public void Call(IDomainService domainService)
        {
            var psi = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = config.command,
                Arguments = config.arg
            };
            Process.Start(psi);
        }

        public RunCommand(Config cfg)
        {
            config = cfg;
        }

        public class Config
        {
            public string command;
            public string arg;
        }
    }
}