# DDNS.CloudFlare
### 一款极简、可自定义配置的、可自动运行的、使用.NET开发的DDNS小程序
##### 可使用Server文件夹搭建nodejs服务器，实现掉线通知telegram
环境搭建：
```bash
npm install axios --save
node index.js
```
默认端口 23100
可直接装进docker
搭建服务器后请务必访问一次 ``` http://example.com/?cdata=c ```（如搭配使用DDNS则需要自己修改 ``` Helper.Push ``` 方法中requestUri地址）

Todo : 通过配置文件修改
##### 配置文件介绍

```json
{
  "email": "mail@example.com",
  "apiKey": "xxxxxxxxx",
  "rootDomain": "example.com",
  "ddnsDomain": "ddns.example.com",
  "useIPv6": false,
  "useConstIP": false,
  "constIP": "",
  "restartTime": 600,
  "autoRun": true
}
```

email，你的cloudflare电子邮件

apikey，你的全局key，获取： Cloudflare -> 我的个人资料 -> API令牌 -> Global API Key

rootDomain，根域名

ddnsDomain，需要解析的二级域名 或者 根域名

useIPv6，是否使用IPv6

restartTime，重新运行间隔，单位：秒

autoRun，是否自动重新运行
