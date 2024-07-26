using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Netko.NetDisk.Baidu;

namespace Netko.NetDisk.Baidu
{
    internal class BaiduShare(Baidu account)
    {
        public Baidu Account = account;

        public const string app_id = "250528";
        public const string channel = "chunlei";
        public const string clienttype = "0";
        public const string netdisk_user_agent = "netdisk";
        public const string broswer_user_agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.52";

        public JsonArray get_share_list(string shareid, string uk, string randsk)
        {
            string url = $"https://pan.baidu.com/share/list?app_id={app_id}&channel={channel}&clienttype=0&desc=1&num=100&order=time&page=1&root=1&shareid={shareid}&showempty=0&uk={uk}&web=1";
            JsonArray sharelist = new JsonArray();
            return sharelist;
        }
    }
}
