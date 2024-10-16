using Avalonia.Controls.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Netko.NetDisk.Baidu
{
    public struct BDDir
    {
        public long Category;
        public long ExtentTinyint7;
        public long FromType;
        public long ID;
        public long IsScene;
        public long LocalCtime;
        public long LocalMtime;
        public long OperID;
        public long OwnerID;
        public long OwnerType;
        public string Path;
        public long pl;
        public string RealCategory;
        public long ServerAtime;
        public long ServerCtime;
        public string Name;
        public long ServerMtime;
        public long Share;
        public long Size;
        public long TkBindID;
        public long UnList;
        public long WpFile;
    }
    public struct BDFile
    {
        public long Category;
        public long ExtentTinyint7;
        public long FromType;
        public long ID;
        public long IsScene;
        public long LocalCtime;
        public long LocalMtime;
        public long OperID;
        public long OwnerID;
        public long OwnerType;
        public string Path;
        public string MD5;
        public long pl;
        public string RealCategory;
        public long ServerAtime;
        public long ServerCtime;
        public string Name;
        public long ServerMtime;
        public long Share;
        public long Size;
        public long TkBindID;
        public long UnList;
        public long WpFile;
    }

    public struct BDFileList
    {
        public string Path;
        public BDFile[] File;
        public BDDir[] Dir;
    }

    public class BaiduFileList(Baidu Account)
    {
        public Baidu BaiduAccount = Account;
        public Dictionary<string, BDFileList> FileListTemp = new Dictionary<string, BDFileList>();
            
        private BDDir ParseDir(JObject item)
        {
            BDDir dir = new BDDir();
            dir.Category = (long)(item["category"] ?? 0);
            dir.ExtentTinyint7 = (long)(item["extent_tinyint7"] ?? 0);
            dir.FromType = (long)(item["from_type"] ?? 0);
            dir.ID = (long)(item["fs_id"] ?? 0);
            dir.IsScene = (long)(item["is_scene"] ?? 0);
            dir.LocalCtime = (long)(item["local_ctime"] ?? 0);
            dir.LocalMtime = (long)(item["local_mtime"] ?? 0);
            dir.OperID = (long)(item["oper_id"] ?? 0);
            dir.OwnerID = (long)(item["owner_id"] ?? 0);
            dir.OwnerType = (long)(item["owner_type"] ?? 0);
            dir.Path = item["path"]?.ToString() ?? "/";
            dir.pl = (long)(item["pl"] ?? 0);
            dir.RealCategory = item["real_category"]?.ToString() ?? "";
            dir.ServerAtime = (long)(item["server_atime"] ?? 0);
            dir.ServerCtime = (long)(item["server_ctime"] ?? 0);
            dir.ServerMtime = (long)(item["server_mtime"] ?? 0);
            dir.Name = item["server_filename"]?.ToString() ?? "";
            dir.Share = (long)(item["share"] ?? 0);
            dir.Size = (long)(item["size"] ?? -1);
            dir.TkBindID = (long)(item["tkbind_id"] ?? 0);
            dir.UnList = (long)(item["unlist"] ?? 0);
            dir.WpFile = (long)(item["wpfile"] ?? 0);
            return dir;
        }
        private BDFile ParseFile(JObject item)
        {
            BDFile file = new BDFile();
            file.Category = (long)(item["category"] ?? 0);
            file.ExtentTinyint7 = (long)(item["extent_tinyint7"] ?? 0);
            file.FromType = (long)(item["from_type"] ?? 0);
            file.ID = (long)(item["fs_id"] ?? 0);
            file.IsScene = (long)(item["is_scene"] ?? 0);
            file.LocalCtime = (long)(item["local_ctime"] ?? 0);
            file.LocalMtime = (long)(item["local_mtime"] ?? 0);
            file.OperID = (long)(item["oper_id"] ?? 0);
            file.OwnerID = (long)(item["owner_id"] ?? 0);
            file.OwnerType = (long)(item["owner_type"] ?? 0);
            file.Path = item["path"]?.ToString() ?? "/";
            file.pl = (long)(item["pl"] ?? 0);
            file.RealCategory = item["real_category"]?.ToString() ?? "";
            file.ServerAtime = (long)(item["server_atime"] ?? 0);
            file.ServerCtime = (long)(item["server_ctime"] ?? 0);
            file.ServerMtime = (long)(item["server_mtime"] ?? 0);
            file.Name = item["server_filename"]?.ToString() ?? "";
            file.Share = (long)(item["share"] ?? 0);
            file.Size = (long)(item["size"] ?? -1);
            file.TkBindID = (long)(item["tkbind_id"] ?? 0);
            file.UnList = (long)(item["unlist"] ?? 0);
            file.WpFile = (long)(item["wpfile"] ?? 0);
            file.MD5 = item["md5"]?.ToString() ?? "0";
            return file;
        }
        private BDFileList ParseFileList(JArray FileList)
        {
            BDFileList fileList = new BDFileList();
            int totalFiles = FileList.Count(item => (int?)item["isdir"] == 0);
            int totalDirs = FileList.Count(item => (int?)item["isdir"] == 1);
            fileList.File = new BDFile[totalFiles];
            fileList.Dir = new BDDir[totalDirs];
            int file_count = 0, dir_count = 0;
            foreach (JObject item in FileList) {
                if ((int?)item["isdir"] == 0)
                {
                    // File
                    fileList.File[file_count] = ParseFile(item);
                    file_count++;
                }
                else
                {
                    // Dir
                    fileList.Dir[dir_count] = ParseDir(item);
                    dir_count++;
                }
            }
            return fileList;

        }
        public async Task<BDFileList> GetFileList(int page, int num=1000, string path="/")
        {
            path = WebUtility.UrlEncode(path);
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);
            string url = $"https://pan.baidu.com/api/list?dir={path}&page={page}&num={num}&clienttype=8&channel=00000000000000000000000040000001&version=7.45.0.109&devuid=BDIMXV2%2dO%5f9432D545AA3849D19EFD762333A53888%2dC%5f0%2dD%5fE823%5f8FA6%5fBF53%5f0001%5f001B%5f448B%5f4A73%5f734B%2e%2dM%5f088FC3E23825%2dV%5f3EBDE1E0&rand=17a15c3f37fc5bd28a6fea151d1c7c6f67fb1c22&time=1728903683&rand2=b4e5cb179b298309a26dd85e95596c2ac1cd2300&vip=2&logid={log_id}&desc=1&order=time";
            Trace.WriteLine(url);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", Baidu.netdisk_user_agent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            HttpResponseMessage content = await client.GetAsync(url);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Console.WriteLine(task_content.Result);

            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                if (body.ContainsKey("list") && body["list"] is JArray result)
                {
                    return ParseFileList(result);
                }
            }

            BDFileList fileList = new BDFileList();
            return fileList;
            
        }
    }
}
