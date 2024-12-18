using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Netko.NetDisk.Baidu
{

    public class Baidu(string cookie): INetdisk
    {
        public Dictionary<string, string?> Cookie = new Dictionary<string, string?>();      // contain BDUSS & STOKEN & PANPSC
        public string InitCookieString = cookie;
        public bool AccountError = false;  // if login error: true
        public bool Initialed = false;      // if logined: true
        public int Vip = 0;                // vip = 1; svip = 2
        public bool IsVip = false;         // false
        public bool IsSvip = false;        // false
        public string Loginstate = "";        // 1
        public int VipLevel = 0;          // 1
        public bool IsYearVip = false;    // false
        public string BdStoken = "";          // 966aa9b0xx74e3785980d108f0839xxx
        public string Uk = "";                // 
        public string Sign1 = "";             // b1b24c86a6c49dfxxxfd3725c337xxx6aca88252
        public string Sign3 = "";             // d76e889b6aafdxxx3bd56f4d4053a
        public int Timestamp = 0;          // 1718809129
        public string LogId = "";

        public string Name = "";
        public string HeadphotoUrl = "";
        public long StorageTotal = 0;
        public long StorageUsed = 0;
        public long StorageFree = 0;
        public const string AppId = "250528";
        public const string Channel = "chunlei";
        public const string ClientType = "0";
        public const string NetdiskUserAgent = "netdisk";
        public const string BroswerUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.52";

        public string GetParticalCookie(string[] particalKeys)
        {
            string cookieVal = "";
            foreach (string key in particalKeys)
            {
                if (Cookie.ContainsKey(key))
                {
                    if (Cookie[key] != null)
                    {
                        cookieVal = key + "=" + Cookie[key] + "; ";
                    }
                    else
                    {
                        cookieVal += key + "; ";
                    }
                }
                else
                {
                    Trace.WriteLine(key + "doesnt exist");
                }
            }
            return cookieVal;
        }
        public string GetCookie()
        {
            /*
             * turn cookie dict into cookie header
             */
            string cookieVal = "";
            foreach (var key in Cookie.Keys)
            {
                if (Cookie[key] != null)
                {
                    cookieVal += key + "=" + Cookie[key] + "; ";
                }
                else
                {
                    if (key != "" && key != string.Empty)
                    {
                        cookieVal += key + "; ";

                    }

                }
            }
            return cookieVal;
        }

        public void ProcessSubCookie(string subCookie)
        {
            /*
             * 听说有人晕多层嵌套
             * update cookie value
             */
            string[] subCookieS = subCookie.Split(';');
            foreach (var subCookiePart in subCookieS)
            {
                string? key, value;
                if (subCookiePart.Contains("="))
                {
                    key = subCookiePart.Split("=")[0].Trim();
                    value = subCookiePart.Split("=")[1].Trim();
                }
                else
                {
                    key = subCookiePart;
                    value = null;
                }

                Cookie[key] = value;
            }

        }

        // Update cookie
        public void UpdateCookie(HttpResponseHeaders headers)
        {
            /*
             * format cookie to prepare to parse cookie value
             */

            if (headers.Contains("Set-Cookie"))
            {
                foreach (var cookiePart in headers.GetValues("Set-Cookie"))
                {
                    if (cookiePart.Contains(";"))
                    {
                        ProcessSubCookie(cookiePart);
                    }
                    else
                    {
                        ProcessSubCookie(cookiePart + ";");

                    }
                    Console.WriteLine(cookiePart);
                }
            }
        }


        public void debug_info()
        {
            Trace.WriteLine("log_id: " + LogId);
            Trace.WriteLine("account_error: " + AccountError.ToString());
            Trace.WriteLine("initialed: " + Initialed.ToString());
            Trace.WriteLine("vip: " + Vip.ToString());
            /*Trace.WriteLine("is_vip: " + is_vip.ToString());
            Trace.WriteLine("is_svip: " + is_svip.ToString());
            Trace.WriteLine("loginstate: " + loginstate.ToString());*/
            Trace.WriteLine("bdstoken: " + BdStoken);
            Trace.WriteLine("uk: " + Uk);
            Trace.WriteLine("sign1: " + Sign1);
            Trace.WriteLine("sign3: " + Sign3);
            Trace.WriteLine("name: " + Name);
            Trace.WriteLine("Cookie: " + GetCookie());
            Trace.WriteLine("time: " + Timestamp.ToString());
        }
        public AccountInfo GetAccountInfo()
        {
            return new AccountInfo
            {
                InitCookie = InitCookieString,
                Name = Name,
                Token = BdStoken,
                StorageUsed = StorageUsed,
                StorageTotal = StorageTotal,
                StorageFree = StorageFree,
            };
        }
        public async Task<string> refresh_logid()
        {
            string url = "https://pan.baidu.com/";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", BroswerUserAgent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            HttpResponseMessage content = await client.GetAsync(url);

            var headers = content.Headers;
            UpdateCookie(headers);
            LogId = Cookie["BAIDUID_BFESS"] ?? string.Empty;
            return LogId;
        }
        //https://passport.baidu.com/v2/api/getqrcode?lp=pc&qrloginfrom=pc&gid=D37084C-AEB7-493A-992C-9ED15CD1CEEC&callback=tangram_guid_1729166374201&apiver=v3&tt=1729166374830&tpl=netdisk&logPage=traceId%3Apc_loginv5_1729166375%2ClogPage%3Aloginv5&_=1729166374832
        private async Task<bool> GetStorageUsage()
        {
            string url = $"https://pan.baidu.com/api/quota";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", GetCookie());
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");

            HttpResponseMessage content = await client.GetAsync(url);

            UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);

            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                if (body.TryGetValue("result", out object? resultObj))
                {
                    JObject result = JObject.Parse(resultObj.ToString() ?? "{}");
                    StorageTotal = Convert.ToInt64(result["total"]);
                    StorageUsed = Convert.ToInt64(result["used"]);
                    StorageFree = StorageTotal - StorageUsed;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> initial_info()
        {
            /*
             * get vars:
             * - headphoto_url
             * - is_svip
             * - is_vip
             * - loginstate
             * - vip_level
             * - is_year_vip
             * - sign1
             * - sign3
             * - timestamp
             */
            string url = $"https://pan.baidu.com/api/gettemplatevariable?fields=[%22is_svip%22,%22is_vip%22,%22loginstate%22,%22vip_level%22,%22username%22,%22photo%22,%22is_year_vip%22,%22bdstoken%22,%22sign1%22,%22sign3%22,%22timestamp%22]";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", GetCookie());
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");

            HttpResponseMessage content = await client.GetAsync(url);

            UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);

            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                if (body.TryGetValue("result", out object? resultObj))
                {
                    JObject result = JObject.Parse(resultObj.ToString() ?? "{}");
                    HeadphotoUrl = result["photo"]?.ToString() ?? "";
                    IsSvip = (int)(result["is_svip"] ?? 0) == 1 ;
                    IsVip = (int)(result["is_vip"] ?? 0) == 1;
                    Loginstate = result["loginstate"]?.ToString() ?? "";
                    VipLevel = (int)(result["vip_level"] ?? 0);
                    IsYearVip = (int)(result["is_year_vip"] ?? 0) == 1;
                    Sign1 = result["sign1"]?.ToString() ?? "";
                    Sign3 = result["sign3"]?.ToString() ?? "";
                    Timestamp = (int)(result["timestamp"] ?? 0);

                }
                debug_info();
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> GetUkStoken()
        {
            /*
             * Get vars
             * - bdstoken
             * - uk
             * - name
             */
            string url = $"https://pan.baidu.com/api/loginStatus?clienttype={ClientType}&app_id={AppId}&web=1&channel=web&version=0";
            //string url = $"https://pan.baidu.com/api/loginStatus?clienttype=1&app_id=250528&web=1&channel=web&version=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", BroswerUserAgent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Cookie", GetCookie());

            HttpResponseMessage content = await client.GetAsync(url);
            /*var task = Task.Run(() => client.GetAsync(url));
            task.Wait();
            var content = task.Result;*/


            //Console.WriteLine("cookie refreshed" + content.Headers.GetValues("Set-Cookie").ToString());
            UpdateCookie(content.Headers);
            //Console.WriteLine("GETCOOKIE" + GetCookie());

            // get body
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);

            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                // Trace.WriteLine("body.ContainsKey(\"login_info\")" + body.ContainsKey("login_info").ToString());
                if (body.TryGetValue("login_info", out object? loginInfoObj))
                {
                    JObject loginInfo = JObject.Parse(loginInfoObj.ToString() ?? "{}");
                    BdStoken = loginInfo["bdstoken"]?.ToString() ?? "";
                    Uk = loginInfo["uk"]?.ToString() ?? "";
                    Name = loginInfo["username"]?.ToString() ?? "";
                }
                debug_info();
                return true;
            }
            else
            {
                return false;
            }

        }
        private void GetVipNum()
        {
            if (IsSvip)
            {
                Vip = 2;
            }
            else if (IsVip)
            {
                Vip = 1;
            }
            else
            {
                Vip = 0;
            }
        } 
        public async Task Init()
        {
            // Update info from server, this may take some time
            string logId = await refresh_logid();
            if (logId == string.Empty)
            {
                throw new Exception("Refresh logid failed.");
            }
            ProcessSubCookie(InitCookieString);
            Trace.WriteLine("COOKIE: ------" + GetCookie());
            if (!await GetUkStoken())
            {
                throw new Exception("Get UK, Stoken failed.");
            }
            Trace.WriteLine("COOKIE: ------" + GetCookie());
            if (!await GetStorageUsage())
            {
                throw new Exception("Get storage useage failed.");
            }
            if (!await initial_info())
            {
                throw new Exception("Get basic info failed.");
            }
            GetVipNum();

        }
    }
}
