using System;
using System.Collections.Generic;
using System.IO;
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
        public float DownloadProgress;
        public long Downloaded;
        public long TotalSize;
        public int LinkCount;
        public int DownloadingThread;
        public long DownloadBlockSize;

        public bool IsParsing;
        public bool IsPaused;
        public bool IsDownloading;
        public bool IsComplete;
    }
    public enum DownloadMethod
    {
        ParticalDownload,
        MultithreadDownload
    }
    public class DownloadConfig
    {
        public string FileName = string.Empty;
        public string? Url = null;
        public string FilePath = string.Empty;
        public long FileSize = 0;
        public string? Cookie = null;
        public DownloadMethod Method;
        public readonly long BlockSize = 500087;
        public readonly int BufferSize = 1000;
        public int DownloadThread = 1;
        public string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0";
        public Func<Task<List<string>>>? GetUrlFunc = null; 
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
        void ReleaseFile();
        void Run();
    }
    public static class DownloadFactory
    {
        public static IDownload Create(DownloadConfig downloadConfig)
        {
            return downloadConfig.Method switch
            {
                DownloadMethod.ParticalDownload => new Downloader(downloadConfig),
                DownloadMethod.MultithreadDownload => new MultiThreadDownloader(downloadConfig),
                _ => throw new ArgumentException("Method not found.", nameof(downloadConfig.Method)),
            };
        }
    }
    public static class FilePathOperate
    {
        public static void CreatePrentPath(string path)
        {
            if (Path.GetDirectoryName(path) != null)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }
            
            
        }
        public static string NormalizePath(string path)
        {
            return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }
        public static string RemovePrefixPath(string fullPath, string prefixToRemove)
        {
            // 标准化路径分隔符，确保跨平台一致性
            fullPath = fullPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            prefixToRemove = prefixToRemove.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // 确保路径以分隔符结尾，避免错误匹配
            if (!prefixToRemove.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                prefixToRemove += Path.DirectorySeparatorChar;
            }

            // 检查路径是否以指定前缀开头
            if (fullPath.StartsWith(prefixToRemove))
            {
                return fullPath.Substring(prefixToRemove.Length);
            }

            // 如果不包含前缀，返回原路径
            return fullPath;
        }
        private static string GetUniqueFileName(string folder, string fileName)
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
        public static string GetAvailablePath(string? subPath = null, string? fileName = null)
        {
            string resultPath = MeowSetting.GetDownloadPath();
            if (subPath != null)
            {
                resultPath = Path.Combine(resultPath, subPath);
            }
            if (fileName != null)
            {
                resultPath = Path.Combine(resultPath, GetUniqueFileName(resultPath, fileName));
            }
            return resultPath;
        }
    }
    
}