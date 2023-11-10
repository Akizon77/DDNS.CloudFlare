using DDNS.CloudFlare.Interface.DomainService;
using System.Diagnostics;

namespace DDNS.CloudFlare.Interface.Callback
{
    public class RunCommand : ICallbacker
    {
        private readonly Config config;

        public async Task<string> CallAsync(params string[] args)
        {
            await Task.CompletedTask;
            string arg;
            if (args is null)
            {
                arg = config.arg;
            }
            else
            {
                arg = config.arg
                    .Replace("{ip}", args.Length >= 1 ? args[0] : "{ip}")
                    .Replace("{msg}", args.Length >= 2 ? args[1] : "{msg}");
            }
            var psi = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = config.command,
                Arguments = arg
            };
            var p = Process.Start(psi);
            return p is null
                ? "Command execute completed, process info is null"
                : $"Command execute completed, exit code {p.ExitCode}";
        }

        public RunCommand(Config cfg)
        {
            config = cfg;
        }

        public class Config
        {
            public string name = "run_command";
            public string command = "tasklist";
            public string arg = "";
        }
    }
}
