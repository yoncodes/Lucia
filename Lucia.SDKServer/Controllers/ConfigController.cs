using Lucia.Common.Util;
using Lucia.SDKServer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lucia.SDKServer.Controllers
{
    internal class ConfigController : IRegisterable
    {
        private static readonly Dictionary<string, ServerVersionConfig> versions = new();

        static ConfigController()
        {
            // ugly pathing but it works
            versions = JsonConvert.DeserializeObject<Dictionary<string, ServerVersionConfig>>(File.ReadAllText("../../../../Resources/Configs/version_config.json"))!;
        }

        public static void Register(WebApplication app)
        {

            app.MapGet("/prod/client/config/{package}/{version}/standalone/config.tab", (HttpContext ctx) =>
            {
                List<RemoteConfig> remoteConfigs = new();
                ServerVersionConfig versionConfig = versions.GetValueOrDefault((string)ctx.Request.RouteValues["version"]!) ?? versions.First().Value;

               
                foreach (var property in typeof(ServerVersionConfig).GetProperties())
                    remoteConfigs.AddConfig(property.Name, (string)property.GetValue(versionConfig)!);

                remoteConfigs.AddConfig("ApplicationVersion", (string)ctx.Request.RouteValues["version"]!);
                remoteConfigs.AddConfig("Debug", true);
                remoteConfigs.AddConfig("External", true);

                switch ((string?)ctx.Request.RouteValues["package"])
                {
                    case "com.kurogame.haru.kuro":
                        remoteConfigs.AddConfig("Channel", 2);
                        remoteConfigs.AddConfig("PrimaryCdns", "http://prod-zspnsalicdn.kurogame.com/prod");
                        remoteConfigs.AddConfig("SecondaryCdns", "http://prod-zspnstxcdn.kurogame.com/prod");
                        break;
                    default:
                        remoteConfigs.AddConfig("Channel", 5);
                        remoteConfigs.AddConfig("PrimaryCdns", "http://prod-encdn-akamai.kurogame.net/prod|http://prod-encdn-aliyun.kurogame.net/prod");
                        remoteConfigs.AddConfig("SecondaryCdns", "http://prod-encdn-aliyun.kurogame.net/prod");
                        break;
                }

                remoteConfigs.AddConfig("PayCallbackUrl", $"http://{Common.Common.config.GameServer.Host}/api/XPay/KuroPayResult");
                remoteConfigs.AddConfig("CdnInvalidTime", 600);
                remoteConfigs.AddConfig("MtpEnabled", true);
                remoteConfigs.AddConfig("MemoryLimit", 2048);
                remoteConfigs.AddConfig("CloseMsgEncrypt", false);
                remoteConfigs.AddConfig("ServerListStr", $"{Common.Common.config.GameServer.RegionName}#http://{Common.Common.config.GameServer.Host}/api/Login/Login");
                remoteConfigs.AddConfig("AndroidReturnEnabled", false);
                remoteConfigs.AddConfig("AndroidPayCallbackList", $"http://{Common.Common.config.GameServer.Host}/api/XPay/HeroHgAndroidPayResult");
                remoteConfigs.AddConfig("AndroidPayCallbackUrl", $"http://{Common.Common.config.GameServer.Host}/api/XPay/HeroHgAndroidPayResult");
                remoteConfigs.AddConfig("IosPayCallbackUrl", $"http://{Common.Common.config.GameServer.Host}/api/XPay/HeroHgIosPayResult");
                remoteConfigs.AddConfig("DEEnable", true);
                remoteConfigs.AddConfig("DEFilter", "empty");
                remoteConfigs.AddConfig("WatermarkEnabled", false);
                remoteConfigs.AddConfig("PicComposition", "empty");
                remoteConfigs.AddConfig("IosPayCallbackList", $"http://{Common.Common.config.GameServer.Host}/api/XPay/HeroHgIosPayResult");
                remoteConfigs.AddConfig("DeepLinkEnabled", true);
                remoteConfigs.AddConfig("AccountCancellationEnable", false);
                remoteConfigs.AddConfig("DownloadMethod", 1);
                remoteConfigs.AddConfig("PcPayCallbackList", $"http://{Common.Common.config.GameServer.Host}/api/XPay/KuroPayResult");
                remoteConfigs.AddConfig("ParallelDownload", 1);
                remoteConfigs.AddConfig("ParallelQueueSize", "3-7");
                remoteConfigs.AddConfig("WatermarkType", 0);
                remoteConfigs.AddConfig("IsPCPayEnable", true);
                remoteConfigs.AddConfig("ChannelServerListStr", $"1#{Common.Common.config.GameServer.RegionName}#http://{Common.Common.config.GameServer.Host}/api/Login/Login");
                remoteConfigs.AddConfig("IsHeXie", false);
                remoteConfigs.AddConfig("UsingXTableBehaviorNodeOptimize", false);

                string serializedObject = TsvTool.SerializeObject(remoteConfigs);
                SDKServer.log.Info(serializedObject);
                return serializedObject;
            });

        app.MapGet("/prod/client/notice/config/{package}/{version}/LoginNotice.json", (HttpContext ctx) =>
            {
                LoginNotice notice = new()
                {
                    BeginTime = 0,
                    EndTime = 0,
                    HtmlUrl = "/",
                    Id = "1",
                    ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Title = "MAINTENANCE NOTICE\t",
                    LoginPlatformList = new List<int>()
                    {
                        0, 1, 2,
                    }
                };

                string serializedObject = JsonConvert.SerializeObject(notice);
                SDKServer.log.Info(serializedObject);
                return serializedObject;
            });

            app.MapGet("/prod/client/notice/config/{package}/{version}/ScrollTextNotice.json", (HttpContext ctx) =>
            {
                ScrollTextNotice notice = new()
                {
                    Id = "1",
                    ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    BeginTime = 0,
                    EndTime = 0,
                    Content = "[ANNOUNCEMENT] There is no announcement.",
                    ScrollInterval = 300,
                    ScrollTimes = 15,
                    ShowInFight = 1,
                    ShowInPhotograph = 1
                };

                string serializedObject = JsonConvert.SerializeObject(notice);
                SDKServer.log.Info(serializedObject);
                return serializedObject;
            });

            app.MapGet("/prod/client/notice/config/{package}/{version}/ScrollPicNotice.json", (HttpContext ctx) =>
            {
                ScrollPicNotice notice = new()
                {
                    Id = "1",
                    ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Content =
                    [
                        new ScrollPicNotice.NoticeContent()
                        {
                            Id = 0,
                            PicAddr = "0",
                            JumpType = "0",
                            JumpAddr = "0",
                            PicType = "0",
                            Interval = 5,
                            BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                            EndTime = DateTimeOffset.Now.ToUnixTimeSeconds() + 3600 * 24,
                            AppearanceCondition = Array.Empty<dynamic>(),
                            AppearanceDay = Array.Empty<dynamic>(),
                            AppearanceTime = Array.Empty<dynamic>(),
                            DisappearanceCondition = Array.Empty<dynamic>(),
                        }
                    ]
                };

                string serializedObject = JsonConvert.SerializeObject(notice);
                SDKServer.log.Info(serializedObject);
                return serializedObject;
            });

            app.MapGet("/prod/client/notice/config/{package}/{version}/GameNotice.json", (HttpContext ctx) =>
            {
                List<GameNotice> notices = new() {
                    new GameNotice
                {
                    Id = "645b644e27335818fc907787",
                    Title = "HER LAST BOW VERSION UPDATE",
                    Tag = 0,
                    Type = 1,
                    LoginEject = 0,
                    BluePoint = 0,
                    Preload = 0,
                    Order = 98,
                    ModifyTime = 1683778900,
                    BeginTime = 1683590400,
                    EndTime = 1686794100,
                    Content = new List<NoticeContent>
                    {
                        new NoticeContent
                        {
                            Id = 0,
                            Title = "HERLASTBOWVERSIONUPDATE",
                            Url = "client/notice/html/645c6d4f27335818fc907798.html",
                            Order = "1"
                        }
                    }
                },

                new GameNotice
                {
                    Id = "645c5d2727335818fc90778d",
                    Title = "HER LAST BOW PACKS INFO",
                    Tag = 0,
                    Type = 1,
                    LoginEject = 0,
                    BluePoint = 0,
                    Preload = 0,
                    Order = 99,
                    ModifyTime = 1683774759,
                    BeginTime = 1683770400,
                    EndTime = 1685174400,
                    Content = new List<NoticeContent>
                    {
                        new NoticeContent
                        {
                            Id = 0,
                            Title = "HERLASTBOWPACKSINFO",
                            Url = "client/notice/html/645c5d1d27335818fc90778c.html",
                            Order = "1"
                        }
                    }
                },
                };

                string serializedObject = JsonConvert.SerializeObject(notices);
                SDKServer.log.Info(serializedObject);
                return serializedObject;
            });

            app.MapPost("/feedback", (HttpContext ctx) =>
            {
                SDKServer.log.Info("1");
                return "1";
            });
        }
    }
}
