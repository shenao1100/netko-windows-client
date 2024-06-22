using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using Avalonia.Animation.Easings;


namespace Netko.NetDisk.Baidu
{
    


    internal class Baidu(string cookie)
    {
        public bool     account_error = false;  // if login error: true
        public bool     initialed = false;      // if logined: true
        public string   baidu_cookie = cookie;      // contain BDUSS & STOKEN
        public int      vip = 0;                // vip = 1; svip = 2
        public bool     is_vip = false;         // false
        public bool     is_svip = false;        // false
        public string   loginstate = "";        // 1
        public int      vip_level = 0;          // 1
        public bool     is_year_vip = false;    // false
        public string   bdstoken = "";          // 966aa9b0xx74e3785980d108f0839xxx
        public string   uk = "";                // 
        public string   sign1 = "";             // b1b24c86a6c49dfxxxfd3725c337xxx6aca88252
        public string   sign3 = "";             // d76e889b6aafdxxx3bd56f4d4053a
        public int      timestamp = 0;          // 1718809129

        public const string app_id = "250528";
        public const string channel = "chunlei";
        public const string clienttype = "0";
        public const string netdisk_user_agent = "netdisk";
        public const string broswer_user_agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.52";

        public async Task initial_vars()
        {
            string url = "https://pan.baidu.com/api/gettemplatevariable?fields=[%22is_svip%22,%22is_vip%22,%22loginstate%22,%22vip_level%22,%22username%22,%22photo%22,%22is_year_vip%22,%22bdstoken%22,%22sign1%22,%22sign3%22,%22timestamp%22]";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", netdisk_user_agent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", baidu_cookie);
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            var content = await client.GetStringAsync(url);
            Trace.WriteLine(content);
        }

        public string refresh_logid()
        {
            string url = "https://pan.baidu.com/";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", broswer_user_agent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Referer", url);
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            var task = Task.Run(() => client.GetAsync(url));
            task.Wait();
            var content = task.Result;
            var headers = content.Headers;
            foreach ( var header in headers)
            {
                if (header.Key == "Cookie")
                {
                    return header.Value.ToString();
                }
            }
            Trace.WriteLine(content);
            return "";
        }
        public bool initial_info()
        {
            string url = $"https://pan.baidu.com/api/user/getinfo?app_id={app_id}&bdstoken={bdstoken}&channel={channel}&clienttype={clienttype}&need_relation=1&need_selfinfo=1&web=1";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", netdisk_user_agent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", baidu_cookie);
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            var task = Task.Run( () => client.GetStringAsync(url));
            task.Wait();
            var content = task.Result;
            return true;
        }

        
    }
}
