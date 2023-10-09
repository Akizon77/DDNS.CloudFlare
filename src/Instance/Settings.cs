using DDNS.CloudFlare.Interface.Callback;
using DDNS.CloudFlare.Interface.DomainService;
using Newtonsoft.Json.Linq;

namespace DDNS.CloudFlare.Instance
{
    public class Settings
    {
#pragma warning disable CS8618
        private static Settings instance;
        private static readonly object lockObject = new object();
        private static readonly string path = "ddns_config.json";

        [Obsolete("Do not create a new instance, it is a singleton run")]
        public Settings()
        { }

        public static Settings CreateNew()
        {
            instance = new Settings();
            var contents = JObject.FromObject(instance).ToString();
            File.WriteAllText(path, contents);
            return Instance;
        }

        public void Save()
        {
            var contents = JObject.FromObject(Instance).ToString();
            File.WriteAllText(path, contents);
        }

        private static Settings Load()
        {
            try
            {
                var contents = File.ReadAllText(path);
                return JObject.Parse(contents).ToObject<Settings>();
            }
            catch (Exception)
            {
                return new Settings();
            }
        }

        public static Settings Instance
        {
            get
            {
                // 使用双重锁定确保线程安全
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = Load();
                        }
                    }
                }
                return instance;
            }
        }

        public static Settings GetInstance() => Instance;

        // 其他设置相关的方法和属性
        public bool auto_restart = true;

        public int intervals = 600;
        public string domain_service = "cloudflare";
#if DEBUG
        public bool debug = true;
#else
        public bool debug = false;
#endif
        public URLCallback.Config callback_url = new();
        public RunCommand.Config callback_action = new();
        public Cloudflare.Config cloudflare = new();
    }
}