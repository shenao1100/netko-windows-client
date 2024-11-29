using Netko.NetDisk.Baidu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Netko.NetDisk
{
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

        Task<bool> CreateDir(string path);
        Task<bool> Rename(string[] file_list, string[] name_list);
        Task<bool> Copy(string[] file_list, string[] name_list, string[] target_path_list);
        Task<bool> Move(string[] file_list, string[] name_list, string[] target_path_list);
        Task<string?> ShareFile(string file_id_list, string password, int period);
        Task<bool> DeleteFile(string file_list);
        Task<BDFileList> GetFileList(int page, int num = 1000, string path = "/", bool clear_select_list = true);
        Task<TaskStatus> GetProgress(string request_id);
        Task<List<string>> GetFileDownloadLink(string path);

    }

}
