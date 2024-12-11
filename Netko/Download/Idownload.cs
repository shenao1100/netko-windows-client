using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Netko.Download
{
    public class Range
    {
        public long from { get; set; }
        public long to { get; set; }
    }
    public class DownloadStatus
    {
        public float downloadProgress;
        public long downloaded;
        public long totalSize;
        public int linkCount;
        public int downloadingThread;
        public long downloadBlockSize;

        public bool isPaused;
        public bool isDownloading;
        public bool isComplete;
    }
    public enum DownloadMethod
    {
        ParticalDownload,
        MultithreadDownload
    }
    public class DownloadConfig
    {
        public string FileName = string.Empty;
        public string Url = string.Empty;
        public string FilePath = string.Empty;
        public long FileSize = 0;
        public string? Cookie = null;
        public DownloadMethod method;
        public long BlockSize = 500087;
        public int BufferSize = 1000;
        public int DownloadThread = 1;
        public string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0";
    }
    public interface IDownload
    {
        void SetCallBack(Action callBack);
        void AddUrl(string url);
        DownloadStatus Status();
        void Pause();
        void Continue();
        void Cancel();
        Task DownloadThread(Range range, string url);
        void releaseFile();
        void Run();
    }
    public static class DownloadFactory
    {
        public static IDownload Create(DownloadConfig downloadCondig)
        {
            return downloadCondig.method switch
            {
                DownloadMethod.ParticalDownload => new Downloader(downloadCondig),
                DownloadMethod.MultithreadDownload => new MultiThreadDownloader(downloadCondig),
                _ => throw new ArgumentException("Method not found.", nameof(downloadCondig.method)),
            };
        }
    }
    public static class FilePathOperate
    {
        public static string GetUniqueFileName(string folder, string fileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            string uniqueFileName = fileName;
            int count = 1;

            while (File.Exists(Path.Combine(folder, uniqueFileName)))
            {
                uniqueFileName = $"{fileNameWithoutExtension} ({count}){extension}";
                count++;
            }
            return uniqueFileName;
        }
        public static string GetAvailablePath(string? sub_path = null, string? file_name = null)
        {
            string ResultPath = MeowSetting.GetDownloadPath();
            if (sub_path != null)
            {
                ResultPath = Path.Combine(ResultPath, sub_path);
            }
            if (file_name != null)
            {
                ResultPath = Path.Combine(ResultPath, GetUniqueFileName(ResultPath, file_name));
            }
            return ResultPath;
        }
    }
    
}