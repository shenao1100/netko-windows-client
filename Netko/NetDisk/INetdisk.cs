using Netko.NetDisk.Baidu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Netko.NetDisk
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
    public class NetdiskResult
    {
        public bool Success;
        public string? Msg;
        public int ResultID;
        public string? TaskID = string.Empty;
    }
    public class AccountInfo
    {
        public string? InitCookie = null;
        public string? Name = null;
        public string? Token = null;
    }
    public class TaskStatus
    {
        public int Progress;
        public bool isError;
        public TaskStatusIndicate Status;
    }
    public interface INetdisk
    {
        AccountInfo GetAccountInfo();
        string GetParticalCookie(string[] partical_keys);
        string GetCookie();
        void ProcessSubCookie(string cookie_);
        void UpdateCookie(HttpResponseHeaders headers);
        void debug_info();
        Task<string> refresh_logid();
        Task<bool> initial_info();
        Task init();

    }
    public interface IFileList
    {
        BDFileList GetSelectedItem();
        bool ToggleSelectDir(BDDir Dir);
        bool DirIsSelected(BDDir Dir);
        bool FileIsSelected(BDFile File);
        bool ToggleSelectFile(BDFile file);
        string IntegrateFilelist(BDFile[]? files, BDDir[]? dirs);
        string IntegrateIDlist(BDFile[]? files, BDDir[]? dirs);

        Task<NetdiskResult> CreateDir(string path);
        Task<NetdiskResult> Rename(string[] file_list, string[] name_list, bool isAsync = false);
        Task<NetdiskResult> Copy(string[] file_list, string[] name_list, string[] target_path_list, bool isAsync = false);
        Task<NetdiskResult> Move(string[] file_list, string[] name_list, string[] target_path_list, bool isAsync = false);
        Task<string?> ShareFile(string file_id_list, string password, int period);
        Task<NetdiskResult> DeleteFile(string file_list, bool isAsync = false);
        Task<BDFileList> GetFileList(int page, int num = 1000, string path = "/", bool clear_select_list = true);
        Task<TaskStatus> GetProgress(string request_id);
        Task<List<string>> GetFileDownloadLink(string path);

    }

}
