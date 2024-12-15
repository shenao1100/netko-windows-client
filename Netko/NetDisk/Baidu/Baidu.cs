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
        public Dictionary<string, string?> cookie = new Dictionary<string, string?>();      // contain BDUSS & STOKEN & PANPSC
        public string init_cookie_string = cookie;
        public bool account_error = false;  // if login error: true
        public bool initialed = false;      // if logined: true
        public int vip = 0;                // vip = 1; svip = 2
        public bool is_vip = false;         // false
        public bool is_svip = false;        // false
        public string loginstate = "";        // 1
        public int vip_level = 0;          // 1
        public bool is_year_vip = false;    // false
        public string bdstoken = "";          // 966aa9b0xx74e3785980d108f0839xxx
        public string uk = "";                // 
        public string sign1 = "";             // b1b24c86a6c49dfxxxfd3725c337xxx6aca88252
        public string sign3 = "";             // d76e889b6aafdxxx3bd56f4d4053a
        public int timestamp = 0;          // 1718809129
        public string log_id = "";

        public string name = "";
        public string headphoto_url = "";
        public long storage_total = 0;
        public long storage_used = 0;
        public long storage_free = 0;
        public const string app_id = "250528";
        public const string channel = "chunlei";
        public const string clienttype = "0";
        public const string netdisk_user_agent = "netdisk";
        public const string broswer_user_agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.52";

        public string GetParticalCookie(string[] particalKeys)
        {
            string cookieVal = "";
            foreach (string key in particalKeys)
            {
                if (cookie.ContainsKey(key))
                {
                    if (cookie[key] != null)
                    {
                        cookieVal = key + "=" + cookie[key] + "; ";
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
            foreach (var key in cookie.Keys)
            {
                if (cookie[key] != null)
                {
                    cookieVal += key + "=" + cookie[key] + "; ";
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

                cookie[key] = value;
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
            Trace.WriteLine("log_id: " + log_id);
            Trace.WriteLine("account_error: " + account_error.ToString());
            Trace.WriteLine("initialed: " + initialed.ToString());
            Trace.WriteLine("vip: " + vip.ToString());
            /*Trace.WriteLine("is_vip: " + is_vip.ToString());
            Trace.WriteLine("is_svip: " + is_svip.ToString());
            Trace.WriteLine("loginstate: " + loginstate.ToString());*/
            Trace.WriteLine("bdstoken: " + bdstoken);
            Trace.WriteLine("uk: " + uk);
            Trace.WriteLine("sign1: " + sign1);
            Trace.WriteLine("sign3: " + sign3);
            Trace.WriteLine("name: " + name);
            Trace.WriteLine("Cookie: " + GetCookie());
            Trace.WriteLine("time: " + timestamp.ToString());
        }
        public AccountInfo GetAccountInfo()
        {
            return new AccountInfo
            {
                InitCookie = init_cookie_string,
                Name = name,
                Token = bdstoken,
                StorageUsed = storage_used,
                StorageTotal = storage_total,
                StorageFree = storage_free,
            };
        }
        public async Task<string> refresh_logid()
        {
            string url = "https://pan.baidu.com/";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", broswer_user_agent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            HttpResponseMessage content = await client.GetAsync(url);

            var headers = content.Headers;
            UpdateCookie(headers);
            log_id = cookie["BAIDUID_BFESS"] ?? string.Empty;
            return log_id;
        }
        //https://passport.baidu.com/v2/api/getqrcode?lp=pc&qrloginfrom=pc&gid=D37084C-AEB7-493A-992C-9ED15CD1CEEC&callback=tangram_guid_1729166374201&apiver=v3&tt=1729166374830&tpl=netdisk&logPage=traceId%3Apc_loginv5_1729166375%2ClogPage%3Aloginv5&_=1729166374832
        private async Task<bool> GetStorageUsage()
        {
            string url = $"https://pan.baidu.com/api/quota";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", netdisk_user_agent);
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
                if (body.TryGetValue("result", out object? resultObj) && resultObj != null)
                {
                    JObject result = JObject.Parse(resultObj.ToString() ?? "{}");
                    storage_total = Convert.ToInt64(result["total"]);
                    storage_used = Convert.ToInt64(result["used"]);
                    storage_free = storage_total - storage_used;
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
            client.DefaultRequestHeaders.Add("User-Agent", netdisk_user_agent);
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
                if (body.TryGetValue("result", out object? resultObj) && resultObj != null)
                {
                    JObject result = JObject.Parse(resultObj.ToString() ?? "{}");
                    headphoto_url = result["photo"]?.ToString() ?? "";
                    is_svip = (int)(result["is_svip"] ?? 0) == 1 ? true : false;
                    is_vip = (int)(result["is_vip"] ?? 0) == 1 ? true : false;
                    loginstate = result["loginstate"]?.ToString() ?? "";
                    vip_level = (int)(result["vip_level"] ?? 0);
                    is_year_vip = (int)(result["is_year_vip"] ?? 0) == 1 ? true : false;
                    sign1 = result["sign1"]?.ToString() ?? "";
                    sign3 = result["sign3"]?.ToString() ?? "";
                    timestamp = (int)(result["timestamp"] ?? 0);

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
            string url = $"https://pan.baidu.com/api/loginStatus?clienttype={clienttype}&app_id={app_id}&web=1&channel=web&version=0";
            //string url = $"https://pan.baidu.com/api/loginStatus?clienttype=1&app_id=250528&web=1&channel=web&version=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", broswer_user_agent);
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
                if (body.TryGetValue("login_info", out object? logininfoObj) && logininfoObj != null)
                {
                    JObject loginInfo = JObject.Parse(logininfoObj?.ToString() ?? "{}");
                    bdstoken = loginInfo["bdstoken"]?.ToString() ?? "";
                    uk = loginInfo["uk"]?.ToString() ?? "";
                    name = loginInfo["username"]?.ToString() ?? "";
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
            if (is_svip)
            {
                vip = 2;
            }
            else if (is_vip)
            {
                vip = 1;
            }
            else
            {
                vip = 0;
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
            ProcessSubCookie(init_cookie_string);
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
