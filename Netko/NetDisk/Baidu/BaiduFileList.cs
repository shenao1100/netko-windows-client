using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Netko.Download;

namespace Netko.NetDisk.Baidu
{
    /// <summary>
    /// For use in serialize json obj in Rename data
    /// </summary>
    public class RenameItem
    {
        public string? path { get; set; }
        public string? newName { get; set; }
    }
    /// <summary>
    /// For use in serial json obj in move data
    /// </summary>
    public class MoveItem
    {
        public string? path { set; get; }
        public string? dest { set; get; }
        public string? newName { get; set; }
    }
    
    public class BaiduFileList(INetdisk account): IFileList
    {

        private readonly Baidu _baiduAccount = (Baidu)account;
        public Dictionary<string, FileList> FileListTemp = new Dictionary<string, FileList>();

        public List<NetDir> _selectDirList = new List<NetDir>();
        public List<NetFile> _selectFileList = new List<NetFile>();
        private Dictionary<string, TaskStatus> _localTaskStatus = new Dictionary<string, TaskStatus>();
        
        const string Channel = "00000000000000000000000040000001";
        const string ChannelShortS = "chunlei";
        const string Version = "7.46.5.113";
        readonly string _devuid = WebUtility.UrlEncode("BDIMXV2-O_" + GenerateSha1Hash(DateTimeOffset.Now.ToUnixTimeSeconds().ToString()).ToUpper() + "-C_0-D_EF93_EFA6_BE93_0001_001B_448B_4A73_736B.-M_088FC3E93899-V_3FFFE1E0");
        readonly string _rand = GenerateSha1Hash(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + "meow_rand1");
        readonly string _time = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        readonly string _rand2 = GenerateSha1Hash(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + "meow");

        public const string AppId = "250528";
        private const string Clienttype = "0";
        private const string NetdiskUserAgent = "netdisk";
        public const string BrowserUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36 Edg/107.0.1418.52";

        public AccountInfo GetAccountInfo()
        {
            return new AccountInfo
            {
                InitCookie = _baiduAccount.InitCookieString,
                Name = _baiduAccount.Name,
                Token = _baiduAccount.BdStoken,
                StorageUsed = _baiduAccount.StorageUsed,
                StorageTotal = _baiduAccount.StorageTotal,
                StorageFree = _baiduAccount.StorageFree,
            };
        }
        static string GenerateSha1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())  // 创建 SHA-1 实例
            {
                // 将输入文本转换为字节数组
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // 计算哈希值
                byte[] hashBytes = sha1.ComputeHash(inputBytes);

                // 将哈希值转换为十六进制字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
        private NetDir ParseDir(JObject item)
        {
            NetDir dir = new NetDir();
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
        private NetFile ParseFile(JObject item)
        {
            NetFile file = new NetFile();
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
        private FileList ParseFileList(JArray jArrayFileList)
        {
            FileList fileList = new FileList();
            int totalFiles = jArrayFileList.Count(item => (int?)item["isdir"] == 0);
            int totalDirs = jArrayFileList.Count(item => (int?)item["isdir"] == 1);
            fileList.File = new List<NetFile>();
            fileList.Dir = new List<NetDir>();
            //int fileCount = 0, dirCount = 0;
            foreach (JObject item in jArrayFileList) {
                if ((int?)item["isdir"] == 0)
                {
                    // File
                    fileList.File.Add(ParseFile(item));
                    //fileCount++;
                }
                else if ((int?)item["isdir"] == 1)
                {
                    // Dir
                    fileList.Dir.Add(ParseDir(item));
                    //dirCount++;
                }
            }
            return fileList;

        }

        public FileList GetSelectedItem()
        {
            if(_selectDirList.Count == 0 && _selectFileList.Count == 0)
            {
                return new FileList()
                {
                    File = new List<NetFile>(),
                    Dir = new List<NetDir>()
                };
            }
            FileList fileList = new FileList();
            fileList.File = _selectFileList;
            fileList.Dir = _selectDirList;
            return fileList;
        }

        public bool ToggleSelectDir(NetDir dir)
        {
            if (_selectDirList.Contains(dir))
            {
                _selectDirList.Remove(dir);
                return false;
            }
            else
            {
                _selectDirList.Add(dir);
                return true;
            }
        }

        public bool DirIsSelected(NetDir dir)
        {
            if (_selectDirList.Contains(dir)) { return true; } else { return false; }
        }

        public bool FileIsSelected(NetFile file)
        {
            if (_selectFileList.Contains(file)) { return true; } else { return false; }
        }

        public bool ToggleSelectFile(NetFile file)
        {
            if (_selectFileList.Contains(file))
            {
                _selectFileList.Remove(file);
                return false;
            }
            else
            {
                _selectFileList.Add(file);
                return true;
            }
        }
        /// <summary>
        /// Convert File and Dir NAME list into list string that server can identify.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="dirs"></param>
        /// <returns>like: "[\"/新建文件夹\"]"</returns>
        public string IntegrateFilelist(List<NetFile>? files, List<NetDir>? dirs)
        {
            string result = "[";
            int count = 0, totalCount = 0;
            if (files != null)
            {
                totalCount += files.Count();
            }
            if (dirs != null)
            {
                totalCount += dirs.Count();
            }
            if (files != null) {
                foreach (NetFile file in files)
                {
                    count++;
                    if (count == totalCount)
                    {
                        result += $"\"{file.Path}\"";
                    }
                    else
                    {
                        result += $"\"{file.Path}\",";
                    }
                }
            }
            if (dirs != null)
            {
                foreach (NetDir dir in dirs)
                {
                    count++;
                    if (count == totalCount)
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
        public string IntegrateIDlist(List<NetFile>? files, List<NetDir>? dirs)
        {
            string result = "[";
            int count = 0, totalCount = 0;
            if (files != null)
            {
                totalCount += files.Count();
            }
            if (dirs != null)
            {
                totalCount += dirs.Count();
            }

            if (files != null)
            {
                foreach (NetFile file in files)
                {
                    count++;
                    if (count == totalCount)
                    {
                        result += $"{file.ID}";
                    }
                    else
                    {
                        result += $"{file.ID},";
                    }
                }
            }
            if (dirs != null)
            {
                foreach (NetDir dir in dirs)
                {
                    count++;
                    if (count == totalCount)
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

        private void ChangeLocalTaskStatus(string taskId, 
            string message,
            int progress, 
            bool isError, 
            TaskStatusIndicate status)
        {
            _localTaskStatus[taskId] = new TaskStatus()
            {
                Message = message,
                TaskId = taskId,
                Progress = progress,
                IsError = isError,
                Status = status
            };
        }
        public async Task<NetdiskResult> CreateDir(string path)
        {
            
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string url = $"https://pan.baidu.com/api/create?clienttype={Clienttype}&channel={Channel}&version={Version}&devuid={_devuid}&rand={_rand}&time={_time}&rand2={_rand2}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
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
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            NetdiskResult netdiskResult = new NetdiskResult();
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                netdiskResult.Success = true;
                netdiskResult.Msg = null;
                netdiskResult.ResultId = Convert.ToInt32(body["errno"]);
                netdiskResult.TaskId = (body.ContainsKey("taskid")) ? Convert.ToString(body["taskid"]) : null;
                return netdiskResult;
            }
            else {
                netdiskResult.Success = false;
                netdiskResult.ResultId = (body != null) ? Convert.ToInt32(body["errno"]) : -1;
                netdiskResult.Msg = (body != null && body.ContainsKey("show_msg")) ? Convert.ToString("show_msg") : null;
                return netdiskResult;

            }

        }

        public async Task<NetdiskResult> Rename(string[] fileList, string[] nameList, bool isAsync = false)
        {
            // pack json data
            var data = new List<RenameItem>();
            if (fileList.Length == nameList.Length)
            {
                for (int i = 0; i < nameList.Length; i++)
                {
                    data.Add(new RenameItem { newName = nameList[i], path = fileList[i] });
                }
            }
            else
            {
                return new NetdiskResult
                {
                    Success = false,
                    Msg = "列表长度不一致",
                    ResultId = -1
                };
            }
            // serialize json data
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            // pack to key-value data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", jsonString)
            });
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string asyncParam = isAsync ? "2" : "1";
            string url = $"https://pan.baidu.com/api/filemanager?opera=rename&async={asyncParam}&onnest=fail&channel={ChannelShortS}&web=1&app_id=250528&bdstoken={_baiduAccount.BdStoken}&logid={logId}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            HttpResponseMessage content = await client.PostAsync(url, formData);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            NetdiskResult netdiskResult = new NetdiskResult();

            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                netdiskResult.Success = true;
                netdiskResult.Msg = null;
                netdiskResult.ResultId = Convert.ToInt32(body["errno"]);
                netdiskResult.TaskId = (body.ContainsKey("taskid")) ? Convert.ToString(body["taskid"]) : null;
                return netdiskResult;
            }
            else
            {
                netdiskResult.Success = false;
                netdiskResult.ResultId = (body != null) ? Convert.ToInt32(body["errno"]) : -1;
                netdiskResult.Msg = (body != null && body.ContainsKey("show_msg")) ? Convert.ToString("show_msg") : null;
                return netdiskResult;

            }
        }
        public async Task<NetdiskResult> Copy(string[] fileList, string[] nameList, string[] targetPathList, bool isAsync=false)
        {
            // pack json data
            var data = new List<MoveItem>();
            if (fileList.Length == nameList.Length && nameList.Length == targetPathList.Length)
            {
                for (int i = 0; i < nameList.Length; i++)
                {
                    data.Add(new MoveItem { newName = nameList[i], path = fileList[i], dest = targetPathList[i] });
                }
            }
            else
            {
                return new NetdiskResult
                {
                    Success = false,
                    Msg = "列表长度不一致",
                    ResultId = -1
                };
            }
            // serialize json data
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            // pack to key-value data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", jsonString)
            });
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string asyncParam = isAsync ? "2" : "1";
            string url = $"https://pan.baidu.com/api/filemanager?opera=copy&async={asyncParam}&onnest=fail&channel={ChannelShortS}&web=1&app_id=250528&bdstoken={_baiduAccount.BdStoken}&logid={logId}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            HttpResponseMessage content = await client.PostAsync(url, formData);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            NetdiskResult netdiskResult = new NetdiskResult();

            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                netdiskResult.Success = true;
                netdiskResult.Msg = null;
                netdiskResult.ResultId = Convert.ToInt32(body["errno"]);
                netdiskResult.TaskId = (body.ContainsKey("taskid")) ? Convert.ToString(body["taskid"]) : null;
                return netdiskResult;
            }
            else
            {
                netdiskResult.Success = false;
                netdiskResult.ResultId = (body != null) ? Convert.ToInt32(body["errno"]) : -1;
                netdiskResult.Msg = (body != null && body.ContainsKey("show_msg")) ? Convert.ToString("show_msg") : null;
                return netdiskResult;

            }
        }

        public async Task<NetdiskResult> Move(string[] fileList, string[] nameList, string[] targetPathList, bool isAsync = false)
        {
            // pack json data
            var data = new List<MoveItem>();
            if (fileList.Length == nameList.Length && nameList.Length == targetPathList.Length)
            {
                for (int i = 0; i < nameList.Length; i++)
                {
                    data.Add(new MoveItem { newName = nameList[i], path = fileList[i], dest = targetPathList[i] });
                }
            }
            else
            {
                return new NetdiskResult
                {
                    Success = false,
                    Msg = "列表长度不一致",
                    ResultId = -1
                };
            }
            // serialize json data
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            // pack to key-value data
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", jsonString)
            });
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string asyncParam = isAsync ? "2" : "1";

            string url = $"https://pan.baidu.com/api/filemanager?opera=move&async={asyncParam}&onnest=fail&channel={ChannelShortS}&web=1&app_id=250528&bdstoken={_baiduAccount.BdStoken}&logid={logId}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            HttpResponseMessage content = await client.PostAsync(url, formData);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            NetdiskResult netdiskResult = new NetdiskResult();

            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                netdiskResult.Success = true;
                netdiskResult.Msg = null;
                netdiskResult.ResultId = Convert.ToInt32(body["errno"]);
                netdiskResult.TaskId = (body.ContainsKey("taskid")) ? Convert.ToString(body["taskid"]) : null;
                return netdiskResult;
            }
            else
            {
                netdiskResult.Success = false;
                netdiskResult.ResultId = (body != null) ? Convert.ToInt32(body["errno"]) : -1;
                netdiskResult.Msg = (body != null && body.ContainsKey("show_msg")) ? Convert.ToString("show_msg") : null;
                return netdiskResult;
            }
        }

        /// <summary>
        /// Generate share url
        /// </summary>
        /// <param name="fileIdList">[123, 456...]</param>
        /// <param name="password">4 digs combination of number and char</param>
        /// <param name="period">0: forever, 1: one day, 365: 365days...</param>
        /// <returns></returns>
        public async Task<string?> ShareFile(string fileIdList, string password, int period)
        {
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);

            string url = $"https://pan.baidu.com/share/set?channel={Channel}&clienttype=0&web=1&web=1&app_id=250528&bdstoken={_baiduAccount.BdStoken}&logid={logId}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            Trace.WriteLine(fileIdList);
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("fid_list", fileIdList),
                new KeyValuePair<string, string>("channel_list", "[]"),
                new KeyValuePair<string, string>("period", period.ToString()),
                new KeyValuePair<string, string>("pwd", password),
                new KeyValuePair<string, string>("schannel", "4"),

            });

            HttpResponseMessage content = await client.PostAsync(url, formData);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);

            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                return Convert.ToString(body["link"]);
            }
            else { return null; }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileList">like "[\"/新建文件夹\"]"</param>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        public async Task<NetdiskResult> DeleteFile(string fileList, bool isAsync = false)
        {
            //long timestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            //string time_stamp = timestampSeconds.ToString();
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string asyncParam = isAsync ? "2" : "1";

            string url = $"https://pan.baidu.com/api/filemanager?opera=delete&async={asyncParam}&onnest=fail&channel={ChannelShortS}&web=1&app_id=250528&bdstoken={_baiduAccount.BdStoken}&logid={logId}&clienttype=0";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            //client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("filelist", fileList)
            });
            
            HttpResponseMessage content = await client.PostAsync(url, formData);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            Trace.WriteLine(taskContent.Result);
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            NetdiskResult netdiskResult = new NetdiskResult();

            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                netdiskResult.Success = true;
                netdiskResult.Msg = null;
                netdiskResult.ResultId = Convert.ToInt32(body["errno"]);
                netdiskResult.TaskId = (body.ContainsKey("taskid")) ? Convert.ToString(body["taskid"]) : null;
                return netdiskResult;
            }
            else
            {
                netdiskResult.Success = false;
                netdiskResult.ResultId = (body != null) ? Convert.ToInt32(body["errno"]) : -1;
                netdiskResult.Msg = (body != null && body.ContainsKey("show_msg")) ? Convert.ToString("show_msg") : null;
                return netdiskResult;

            }

        }
        public async Task<FileList> GetFileList(int page, int num=1000, string path="/", bool clearSelectList=true)
        {
            if (clearSelectList)
            {
                _selectDirList.Clear();
                _selectFileList.Clear();
            }
            path = WebUtility.UrlEncode(path);
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string url = $"https://pan.baidu.com/api/list?dir={path}&page={page}&num={num}&clienttype=8&channel={Channel}&version=7.45.0.109&devuid={_devuid}&rand={_rand}&time={_time}&rand2={_rand2}&vip={_baiduAccount.Vip}&logid={logId}&desc=1&order=time";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            HttpResponseMessage content = await client.GetAsync(url);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();

            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                if (body.ContainsKey("list") && body["list"] is JArray result)
                {
                    return ParseFileList(result);
                }
            }

            FileList fileList = new FileList();
            return fileList;
            
        }

        public async Task<TaskStatus> GetProgress(string requestId)
        {
            string url = $"https://pan.baidu.com/share/taskquery?taskid={requestId}&clienttype=0&app_id=250528&web=1";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            client.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            client.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            HttpResponseMessage content = await client.GetAsync(url);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();

            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            TaskStatus status = new TaskStatus();
            if (body != null && Convert.ToInt32(body["errno"]) == 0)
            {
                if (body.ContainsKey("status") && body.ContainsKey("task_errno"))
                {
                    string statusStr = Convert.ToString(body["status"])!;
                    switch (statusStr.ToLower())
                    {
                        case "running":
                            status.Status = TaskStatusIndicate.Working;
                            status.Progress = Convert.ToInt32(body["progress"]);

                            break;
                        case "success":
                            status.Status = TaskStatusIndicate.Done;
                            status.Progress = 100;

                            break;
                        default:
                            status.Status = TaskStatusIndicate.InQueue;
                            status.Progress = 0;

                            break;

                    }
                    status.IsError = false;
                    return status;
                }
            }

            status.IsError = true;
            return status;
        }
        /// <summary>
        /// Check if file is legal to download
        /// </summary>
        /// <param name="path"></param>
        private async void downloadPreProcess(string path)
        {
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string newLogid = WebUtility.UrlEncode($"=MTczMTQxMjU5NSw4Nw==&path{path}");

            path = WebUtility.UrlEncode(path);
            string url = $"https://pan.baidu.com/api/checkapl/download?clienttype=8&channel={Channel}&version=7.46.5.113&devuid={_devuid}&rand={_rand}&time={_time}&rand2={_rand2}&vip=0&logid={newLogid}";
            using var nclient = new HttpClient();
            nclient.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            nclient.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            nclient.DefaultRequestHeaders.Add("Accept", "*/*");
            nclient.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            nclient.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            nclient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            nclient.DefaultRequestHeaders.Add("Host", "pan.baidu.com");
            var ndata = new StringContent(" ", Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage ncontent = await nclient.PostAsync(url, ndata);
            _baiduAccount.UpdateCookie(ncontent.Headers);
            var ntaskContent = Task.Run(() => ncontent.Content.ReadAsStringAsync());
            ntaskContent.Wait();
            Trace.WriteLine(ntaskContent.Result);
        }
        /// <summary>
        /// CMS check for p2s record, has been disabled
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<string> CMS(string path)
        {
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string newLogid = WebUtility.UrlEncode($"=MTczMTQxMjU5NSw4Nw==&path{path}");

            path = WebUtility.UrlEncode(path);
            string url = $"https://pan.baidu.com/cms/fgid?method=query&clienttype=9&version=3.0.20.63&time=1731441395&rand=6f0e1e64572533db2cd6b44a731f5ee58244ffba&devuid=BDIMXV2-O_9432D545AA3849D19EFD762333A53888-C_0-D_E823_8FA6_BF53_0001_001B_448B_4A73_734B.-M_088FC3E23825-V_3EBDE1E0&channel=0&version_app=7.46.5.113&path={path}&uk={_baiduAccount.Uk}&vip=0";

            //string url = $"https://pan.baidu.com/api/checkapl/download?clienttype=8&channel=00000000000000000000000040000001&version=7.46.5.113&devuid={devuid}&rand={rand}&time={time}&rand2={rand2}&vip=0&logid={new_logid}";
            using var nclient = new HttpClient();
            nclient.DefaultRequestHeaders.Add("User-Agent", NetdiskUserAgent);
            nclient.DefaultRequestHeaders.Add("Referer", "https://pan.baidu.com/disk/home");
            nclient.DefaultRequestHeaders.Add("Accept", "*/*");
            nclient.DefaultRequestHeaders.Add("Accept-Language", "zh-cn");
            nclient.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            nclient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            nclient.DefaultRequestHeaders.Add("Host", "pan.baidu.com");

            HttpResponseMessage ncontent = await nclient.GetAsync(url);
            _baiduAccount.UpdateCookie(ncontent.Headers);
            var ntaskContent = Task.Run(() => ncontent.Content.ReadAsStringAsync());
            ntaskContent.Wait();
            Trace.WriteLine(ntaskContent.Result);
            return ntaskContent.Result;
        }
        /// <summary>
        /// Get download link from local netdisk
        /// </summary>
        /// <param name="path"></param>
        public async Task<List<string>> GetFileDownloadLink(string path)
        {
            string logId = WebUtility.UrlEncode(_baiduAccount.LogId);
            string origPath = path;
            path = WebUtility.UrlEncode(path);

            //await CMS(orig_path);
            //https://d.pcs.baidu.com/rest/2.0/pcs/file?app_id=250528&method=locatedownload&check_blue=1&es=1&esl=1&ant=1&path=%2F%5BAcgFun.net%5D%E5%90%8C%E4%BA%BA%E6%B8%B8%E6%88%8F%E5%AE%89%E5%8D%93%E7%89%88No.7961.7z&ver=4.0&dtype=1&err_ver=1.0&ehps=1&eck=1&vip=0&clienttype=17&version=2.2.101.200&channel=0&version_app=12.17.2&apn_id=1_0&freeisp=0&queryfree=0&cuid=BE810DBE5D1FD4327F66DE30AD7ECBBF%7CVJ3ISVMPX&use=0
            //string url = $"https://d.pcs.baidu.com/rest/2.0/pcs/file?app_id=250528&method=locatedownload&check_blue=1&es=1&esl=1&ant=1&path={path}&ver=4.0&dtype=1&err_ver=1.0&ehps=1&eck=1&vip=1&open_pflag=2&vip={BaiduAccount.vip}&dpkg=1&sd=0&clienttype=9&version=3.0.20.63&time={time}&rand={rand}&devuid={devuid}&channel=0&version_app=7.46.5.113";
            string url = $"https://d.pcs.baidu.com/rest/2.0/pcs/file?app_id=250528&method=locatedownload&check_blue=1&es=1&esl=1&ant=1&path={path}&ver=4.0&dtype=1&err_ver=1.0&ehps=1&eck=1&vip=1&open_pflag=2&clienttype=17&version=2.2.101.200&channel=0&version_app=12.17.2&apn_id=1_0&freeisp=0&queryfree=0&cuid=BE810DBE5D1FD4327F66DE30AD7ECBBF%7CVJ3ISVMPX&use=0";


            using var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("User-Agent", "WindowsBaiduYunGuanJia");
            //client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "netdisk;P2SP;3.0.20.63;netdisk;7.46.5.113;PC;PC-Windows;10.0.22631;WindowsBaiduYunGuanJia");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "netdisk;P2SP;2.2.101.200;netdisk;12.17.2;PGEM10;android-android;9;JSbridge4.4.0;jointBridge;1.1.0;");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "");
            client.DefaultRequestHeaders.Add("Cookie", _baiduAccount.GetCookie());
            client.DefaultRequestHeaders.Add("Host", "d.pcs.baidu.com");
            var data = new StringContent(" ", Encoding.UTF8, "application/x-www-form-urlencoded");
            HttpResponseMessage content = await client.PostAsync(url, data);
            _baiduAccount.UpdateCookie(content.Headers);
            var taskContent = Task.Run(() => content.Content.ReadAsStringAsync());
            taskContent.Wait();
            //Trace.WriteLine(task_content.Result);
            List<string> link = new List<string>();
            Dictionary<string, object>? body
                = JsonConvert.DeserializeObject<Dictionary<string, object>>(taskContent.Result);
            if (body != null && body["urls"] is JArray urls)
            {
                foreach (JObject keyValuePairs in urls)
                {
                    link.Add(keyValuePairs["url"]!.ToString());
                }
            }

            return link;

        }
        public DownloadConfig ChooseDownloadMethod()
        {
            // PC: netdisk;P2SP;3.0.20.63;netdisk;7.46.5.113;PC;PC-Windows;10.0.22631;WindowsBaiduYunGuanJia

            if (_baiduAccount.IsSvip || _baiduAccount.IsVip)
            {
                return new DownloadConfig
                {
                    Cookie = GetAccountInfo().InitCookie,
                    Method = DownloadMethod.ParticalDownload,
                    DownloadThread = 8,
                    UserAgent = "netdisk;P2SP;2.2.101.200;netdisk;12.17.2;PGEM10;android-android;9;JSbridge4.4.0;jointBridge;1.1.0;"
                };
            }
            else
            {
                return new DownloadConfig
                {
                    Cookie = GetAccountInfo().InitCookie,
                    Method = DownloadMethod.ParticalDownload,
                    DownloadThread = 1,
                    UserAgent = "netdisk;P2SP;2.2.101.200;netdisk;12.17.2;PGEM10;android-android;9;JSbridge4.4.0;jointBridge;1.1.0;"

                };
                 
            }
        }
        
        public async Task<FileList> MapFileList(string path, FileList fileList = new FileList())
        {
            if (fileList.Dir == null && fileList.File == null)
            {
                fileList.Dir = new List<NetDir>();
                fileList.File = new List<NetFile>();
            }
            int page = 0;
            FileList tempFileList;
            while (true)
            {
                page++;
                tempFileList = await GetFileList(page, path:path, clearSelectList:false);
                if (tempFileList.Dir.Count + tempFileList.File.Count < 1000)
                {
                    break;
                }
            }
            foreach (NetFile file in tempFileList.File)
            {
                fileList.File!.Add(file);
            }
            foreach (NetDir dir in tempFileList.Dir)
            {
                fileList = await MapFileList(path:dir.Path, fileList:fileList);
                fileList.Dir.Add(dir);
            }
            
            return fileList;
        }
    }
}
 