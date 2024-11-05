using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Serialization;
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
    
    /// <summary>
    /// For use in serialize json obj in Rename data
    /// </summary>
    public class RenameItem
    {
        public string path { get; set; }
        public string newname { get; set; }
    }
    /// <summary>
    /// For use in serial json obj in move data
    /// </summary>
    public class MoveItem
    {
        public string path { set; get; }
        public string dest { set; get; }
        public string newname { get; set; }
    }

    public class BaiduFileList(Baidu Account)
    {
        public Baidu BaiduAccount = Account;
        public Dictionary<string, BDFileList> FileListTemp = new Dictionary<string, BDFileList>();

        private List<BDDir> selectDirList = new List<BDDir>();
        private List<BDFile> selectFileList = new List<BDFile>();

        const string channel = "00000000000000000000000040000001";
        const string version = "7.46.5.113";
        const string devuid = "BDIMXV2%2dO%5f9432D545AA3849D19EFD762333A53888%2dC%5f0%2dD%5fE823%5f8FA6%5fBF53%5f0001%5f001B%5f448B%5f4A73%5f734B%2e%2dM%5f088FC3E23825%2dV%5f3EBDE1E0";
        const string rand = "27f884dc5f353636874770bc396e1ce6eca29bfc";
        const string time = "1730132298";
        const string rand2 = "14a3c956f6a8daea6e650c72ce7888a6ee0f503d";
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
        public BDFileList? GetSelectedItem()
        {
            if(selectDirList.Count == 0 && selectFileList.Count == 0)
            {
                return null;
            }
            BDFileList fileList = new BDFileList();
            fileList.File = selectFileList.ToArray();
            fileList.Dir = selectDirList.ToArray();
            return fileList;
        }

        public bool ToggleSelectDir(BDDir Dir)
        {
            if (selectDirList.Contains(Dir))
            {
                selectDirList.Remove(Dir);
                return false;
            }
            else
            {
                selectDirList.Add(Dir);
                return true;
            }
        }

        public bool DirIsSelected(BDDir Dir)
        {
            if (selectDirList.Contains(Dir)) { return true; } else { return false; }
        }

        public bool FileIsSelected(BDFile File)
        {
            if (!selectFileList.Contains(File)) { return true; } else { return false; }
        }

        public bool ToggleSelectFile(BDFile file)
        {
            if (selectFileList.Contains(file))
            {
                selectFileList.Remove(file);
                return false;
            }
            else
            {
                selectFileList.Add(file);
                return true;

            }
        }
        /// <summary>
        /// Convert File and Dir NAME list into list string that server can identify.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="dirs"></param>
        /// <returns>like: "[\"/新建文件夹\"]"</returns>
        public string IntegrateFilelist(BDFile[]? files, BDDir[]? dirs)
        {
            string result = "[";
            int count = 0;
            if (files != null) {
                foreach (BDFile file in files)
                {
                    count++;
                    if (count == files.Length)
                    {
                        result += $"\"{file.Path}\"";
                    }
                    else
                    {
                        result += $"\"{file.Path}\",";
                    }
                }
            }
            count = 0;
            if (dirs != null)
            {
                foreach (BDDir dir in dirs)
                {
                    count++;
                    if (count == dirs.Length)
                    {
                        result += $"\"{dir.Path}\"";
                    }
                    else
                    {
                        result += $"\"{dir.Path}\",";
                    }
                }
            }
            result += "]";
            return result;
        }

        /// <summary>
        /// Convert File and Dir ID list into list string that server can identify.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="dirs"></param>
        /// <returns>like: "[123, 456...]"</returns>
        public string IntegrateIDlist(BDFile[]? files, BDDir[]? dirs)
        {
            string result = "[";
            int count = 0;
            if (files != null)
            {
                foreach (BDFile file in files)
                {
                    count++;
                    if (count == files.Length)
                    {
                        result += $"{file.ID}";
                    }
                    else
                    {
                        result += $"{file.ID},";
                    }
                }
            }
            count = 0;
            if (dirs != null)
            {
                foreach (BDDir dir in dirs)
                {
                    count++;
                    if (count == dirs.Length)
                    {
                        result += $"{dir.ID}";
                    }
                    else
                    {
                        result += $"{dir.ID},";
                    }
                }
            }
            result += "]";
            return result;
        }

        public async Task<bool> CreateDir(string path)
        {
            
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);
            string url = $"https://pan.baidu.com/api/create?clienttype={Baidu.clienttype}&channel={channel}&version={version}&devuid={devuid}&rand={rand}&time={time}&rand2={rand2}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", Baidu.netdisk_user_agent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            //path=//新建文件夹&size=0&isdir=1
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("path", path),
                new KeyValuePair<string, string>("size", "0"),
                new KeyValuePair<string, string>("isdir", "1")
            });
            HttpResponseMessage content = await client.PostAsync(url, formData);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return true;
            }
            else { return false; }

        }

        public async Task<bool> Rename(string[] file_list, string[] name_list)
        {
            // pack json data
            var data = new List<RenameItem>();
            if (file_list.Length == name_list.Length)
            {
                for (int i = 0; i < name_list.Length; i++)
                {
                    data.Add(new RenameItem { newname = name_list[i], path = file_list[i] });
                }
            }
            else
            {
                return false;
            }
            // serialize json data
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            // pack to key-value data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", jsonString)
            });
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);
            string url = $"https://pan.baidu.com/api/filemanager?opera=rename&async=1&onnest=fail&channel=chunlei&web=1&app_id=250528&bdstoken={BaiduAccount.bdstoken}&logid={log_id}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            HttpResponseMessage content = await client.PostAsync(url, formData);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Trace.WriteLine(task_content.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return true;
            }
            else { return false; }
        }
        public async Task<bool> Copy(string[] file_list, string[] name_list, string[] target_path_list)
        {
            // pack json data
            var data = new List<MoveItem>();
            if (file_list.Length == name_list.Length && name_list.Length == target_path_list.Length)
            {
                for (int i = 0; i < name_list.Length; i++)
                {
                    data.Add(new MoveItem { newname = name_list[i], path = file_list[i], dest = target_path_list[i] });
                }
            }
            else
            {
                return false;
            }
            // serialize json data
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            // pack to key-value data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", jsonString)
            });
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);
            string url = $"https://pan.baidu.com/api/filemanager?opera=copy&async=1&onnest=fail&channel=chunlei&web=1&app_id=250528&bdstoken={BaiduAccount.bdstoken}&logid={log_id}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            HttpResponseMessage content = await client.PostAsync(url, formData);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Trace.WriteLine(task_content.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return true;
            }
            else { return false; }
        }

        public async Task<bool> Move(string[] file_list, string[] name_list, string[] target_path_list)
        {
            // pack json data
            var data = new List<MoveItem>();
            if (file_list.Length == name_list.Length && name_list.Length == target_path_list.Length)
            {
                for (int i = 0; i < name_list.Length; i++)
                {
                    data.Add(new MoveItem { newname = name_list[i], path = file_list[i], dest = target_path_list[i] });
                }
            }
            else
            {
                return false;
            }
            // serialize json data
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            // pack to key-value data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", jsonString)
            });
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);
            string url = $"https://pan.baidu.com/api/filemanager?opera=move&async=1&onnest=fail&channel=chunlei&web=1&app_id=250528&bdstoken={BaiduAccount.bdstoken}&logid={log_id}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            HttpResponseMessage content = await client.PostAsync(url, formData);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Trace.WriteLine(task_content.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Generate share url
        /// </summary>
        /// <param name="file_id_list">[123, 456...]</param>
        /// <param name="password">4 digs combation of munber and char</param>
        /// <param name="period">0: forever, 1: one day, 365: 365days...</param>
        /// <returns></returns>
        public async Task<string?> ShareFile(string file_id_list, string password, int period)
        {
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);

            string url = $"https://pan.baidu.com/share/set?channel=chunlei&clienttype=0&web=1&channel=chunlei&web=1&app_id=250528&bdstoken={BaiduAccount.bdstoken}&logid={log_id}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("fid_list", file_id_list),
                new KeyValuePair<string, string>("channel_list", "[]"),
                new KeyValuePair<string, string>("period", period.ToString()),
                new KeyValuePair<string, string>("pwd", password),
                new KeyValuePair<string, string>("schannel", "4"),

            });

            HttpResponseMessage content = await client.PostAsync(url, formData);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Trace.WriteLine(task_content.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return Convert.ToString(body["link"]);
            }
            else { return null; }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file_list">like "[\"/新建文件夹\"]"</param>
        /// <returns></returns>
        public async Task<bool> DeleteFile(string file_list)
        {
            //long timestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //string time_stamp = timestampSeconds.ToString();
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);

            string url = $"https://pan.baidu.com/api/filemanager?opera=delete&async=1&onnest=fail&channel=chunlei&web=1&app_id=250528&bdstoken={BaiduAccount.bdstoken}&logid={log_id}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", BaiduAccount.GetCookie());
            //client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", file_list)
            });
            
            HttpResponseMessage content = await client.PostAsync(url, formData);
            BaiduAccount.UpdateCookie(content.Headers);
            var task_content = Task.Run(() => content.Content.ReadAsStringAsync());
            task_content.Wait();
            Trace.WriteLine(task_content.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(task_content.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return true;
            }
            else { return false; }

        }
        public async Task<BDFileList> GetFileList(int page, int num=1000, string path="/", bool clear_select_list=true)
        {
            if (clear_select_list)
            {
                selectDirList.Clear();
                selectFileList.Clear();
            }
            path = WebUtility.UrlEncode(path);
            string log_id = WebUtility.UrlEncode(BaiduAccount.log_id);
            string url = $"https://pan.baidu.com/api/list?dir={path}&page={page}&num={num}&clienttype=8&channel=00000000000000000000000040000001&version=7.45.0.109&devuid=BDIMXV2%2dO%5f9432D545AA3849D19EFD762333A53888%2dC%5f0%2dD%5fE823%5f8FA6%5fBF53%5f0001%5f001B%5f448B%5f4A73%5f734B%2e%2dM%5f088FC3E23825%2dV%5f3EBDE1E0&rand=17a15c3f37fc5bd28a6fea151d1c7c6f67fb1c22&time=1728903683&rand2=b4e5cb179b298309a26dd85e95596c2ac1cd2300&vip=2&logid={log_id}&desc=1&order=time";
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
