# DDNS.CloudFlare
### 一款极简、可自定义配置的、可自动运行的、使用.NET开发的DDNS小程序

##### 配置文件介绍

```json
{
  "email": "xxx@gmail.com",
  "apiKey": "xxxxx",
  "rootDomain": "xxx.xxx",
  "ddnsDomain": "xxx.xxx.xxx",
  "useIPv6": false,
  "useConstIP": false,
  "constIP": "",
  "restartTime": 600,
  "autoRun": true
}
```

email，你的cloudflare电子邮件

apikey，你的全局key

rootDomain，根域名

ddnsDomain，需要解析的二级域名 或者 根域名

useIPv6，是否使用IPv6

restartTime，重新运行间隔

autoRun，是否自动重新运行