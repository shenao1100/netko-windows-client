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
}