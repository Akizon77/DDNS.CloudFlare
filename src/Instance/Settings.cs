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

        public Settings() { }

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
                var j = JObject.Parse(contents);
                //return JObject.Parse(contents).ToObject<Settings>()
                //    ?? throw new ArgumentNullException("Please cheak config file.");
                var s = new List<Cloudflare.Config>();
                foreach ( var item in j["services"] ) 
                {
                    var cbs = new List<object>();
                    foreach (var cb in item["callbacks"])
                    {
                        cbs.Add(cb.ToObject<URLCallback.Config>());
                    }
                    s.Add(new Cloudflare.Config()
                    {
                        ApiKey = item["ApiKey"].ToString(),
                        name = item["name"].ToString(),
                        Email = item["Email"].ToString(),
                        Domain = item["Domain"].ToString(),
                        callbacks = cbs
                    }) ;
                }
                return new()
                {
                    auto_restart = j["auto_restart"].ToObject<bool>(),
                    intervals = j["intervals"].ToObject<int>(),
                    debug = j["debug"].ToObject<bool>(),
                    services = s,
                };
            }
            catch (Exception)
            {
                Logger.Warn("No config file,creating new");
                return CreateNew();
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

        public static List<IDomainService> DomainServices
        {
            get
            {
                //try
                //{
                //    var domainServices = new List<IDomainService>();
                //    Instance.services.ForEach(x =>
                //    {
                //        var jobject = x as JObject;
                //        try
                //        {
                //            domainServices.Add(new Cloudflare(jobject.ToObject<Cloudflare.Config>()));
                //        }
                //        catch { } 
                //    });
                //    return domainServices;
                //}
                //catch (Exception e)
                //{
                //    throw new ArgumentNullException("无法加载域名提供商，请检查配置文件", e);
                //}
                var domainServices = new List<IDomainService>();
                foreach (var item in instance.services)
                {
                    domainServices.Add(new Cloudflare(item));
                }
                return domainServices;
            }
        }

        public static List<ICallbacker> Callbackers(Cloudflare.Config o)
        {
            try
            {
                var callbackers = new List<ICallbacker>();
                o.callbacks.ForEach(x =>
                {
                    if (x is URLCallback.Config c)
                        callbackers.Add(new URLCallback(c));
                    if (x is RunCommand.Config r)
                        callbackers.Add(new RunCommand(r));
                });
                return callbackers;
            }
            catch
            {
                Logger.Warn($"{o.name.ToUpper()}:{o.Email} 无法加载回调，请检查配置文件");
                return new List<ICallbacker>();
            }
        }

        // 其他设置相关的方法和属性
        public bool auto_restart = true;

        public int intervals = 600;
        //public string domain_service = "cloudflare";
#if DEBUG
        public bool debug = true;
#else
        public bool debug = false;
#endif
        public List<Cloudflare.Config> services = new() { new Cloudflare.Config() };
        //public URLCallback.Config callback_url = new();
        //public RunCommand.Config callback_action = new();
        //public Cloudflare.Config cloudflare = new();
    }
}
