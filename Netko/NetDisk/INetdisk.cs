using Netko.Download;
using Netko.NetDisk.Baidu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Netko.NetDisk
{
    public enum NetDiskType
    {

    }

    public struct NetDir : IEquatable<NetDir>
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

        public bool Equals(NetDir other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is NetDir other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(NetDir left, NetDir right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetDir left, NetDir right)
        {
            return !left.Equals(right);
        }
    }

    public struct NetFile : IEquatable<NetFile>
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

        public bool Equals(NetFile other)
        {
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return obj is NetFile && Equals((NetFile)obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public static bool operator ==(NetFile left, NetFile right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(NetFile left, NetFile right)
        {
            return !left.Equals(right);
        }
    }


    public struct FileList
    {
        public string Path;
        public List<NetFile> File;
        public List<NetDir> Dir;
    }
    public class NetdiskResult
    {
        public bool Success;
        public string? Msg;
        public int ResultId;
        public string? TaskId = string.Empty;
    }
    public class AccountInfo
    {
        public string? InitCookie = null;
        public string? Name = null;
        public string? Token = null;
        public long? StorageTotal;
        public long? StorageFree;
        public long? StorageUsed;
    }
    public class TaskStatus
    {
        public int Progress;
        public bool IsError;
        public TaskStatusIndicate Status;
        public string? TaskId = string.Empty;
        public string? Message = string.Empty;
        public Action<TaskStatus>? Callback = null;
    }
     
    public interface INetdisk
    {
        AccountInfo GetAccountInfo();
        string GetParticalCookie(string[] particalKeys);
        string GetCookie();
        void ProcessSubCookie(string subCookie);
        void UpdateCookie(HttpResponseHeaders headers);
        void debug_info();
        Task<string> refresh_logid();
        Task<bool> initial_info();
        Task Init();
    }

    public interface IFileList
    {
        AccountInfo GetAccountInfo();
        FileList GetSelectedItem();
        bool ToggleSelectDir(NetDir dir);
        bool DirIsSelected(NetDir dir);
        bool FileIsSelected(NetFile file);
        bool ToggleSelectFile(NetFile file);
        string IntegrateFilelist(List<NetFile>? files, List<NetDir>? dirs);
        string IntegrateIDlist(List<NetFile>? files, List<NetDir>? dirs);

        Task<NetdiskResult> CreateDir(string path);
        Task<NetdiskResult> Rename(string[] fileList, string[] nameList, bool isAsync = false);
        Task<NetdiskResult> Copy(string[] fileList, string[] nameList, string[] targetPathList, bool isAsync = false);
        Task<NetdiskResult> Move(string[] fileList, string[] nameList, string[] targetPathList, bool isAsync = false);
        Task<string?> ShareFile(string fileIdList, string password, int period);
        Task<NetdiskResult> DeleteFile(string fileList, bool isAsync = false);
        Task<FileList> GetFileList(int page, int num = 1000, string path = "/", bool clearSelectList = true);
        Task<TaskStatus> GetProgress(string requestId);
        Task<List<string>> GetFileDownloadLink(string path);
        DownloadConfig ChooseDownloadMethod();
        Task<FileList> MapFileList(string path, FileList fileList = new FileList());
    }

}
