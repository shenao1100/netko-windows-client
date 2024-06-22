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

        public JsonArray get_share_list(string shareid, string uk, string randsk)
        {
            string url = $"https://pan.baidu.com/share/list?app_id={Account.app_id}&channel={channel}&clienttype=0&desc=1&num=100&order=time&page=1&root=1&shareid={shareid}&showempty=0&uk={uk}&web=1";
            return;
        }
    }
}
