using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Netko.Download
{


    public class MultiThreadDownloader : IDownload
    {
        // 锁
        private readonly object _lock = new object();
        // 阻塞
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(true);
        // 取消信号
        private CancellationTokenSource cts = new CancellationTokenSource();
        private FileStream FileStream { get; set; }
        private int TotalThread { get; set; }
        private int CurrentThread { get; set; }
        private string Url { get; set; }
        private string UserAgent { get; set; }
        private string filePath { get; set; }

        public Action? CallBack { get; set; }

        private Dictionary<int, string> linkList = new Dictionary<int, string>();

        public float downloadProgress = 0;
        public long downloaded = 0;
        public long totalSize = 0;
        public int linkCount = 0;
        public int downloadingThread = 0;
        public long downloadBlockSize = 500087;
        public int BufferSize = 1000;

        public bool isPaused = false;
        public bool isDownloading = true;
        public bool isComplete = false;
        private string? Cookie = null;


        public MultiThreadDownloader(DownloadConfig config)
        {
            TotalThread = config.DownloadThread;
            CurrentThread = 0;
            downloadBlockSize = config.BlockSize;
            BufferSize = config.BufferSize;
            Url = config.Url;
            linkList.Add(linkCount, config.Url);
            linkCount++;

            UserAgent = config.UserAgent;
            totalSize = config.FileSize;
            filePath = config.FilePath;
            Cookie = config.Cookie;

            FileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream.SetLength(totalSize);
        }
        public void AddUrl(string url)
        {
            linkList.Add(linkCount, url);
            linkCount++;

        }
        public void SetCallBack(Action callBack)
        {
            CallBack = callBack;
        }
        private void CalcProgress()
        {
            if (downloaded > 0)
            {
                downloadProgress = (float)downloaded / totalSize;
            }
            if (downloadingThread == 0)
            {
                isDownloading = false;
            }
            else
            {
                isDownloading = true;
            }
            if (downloaded == totalSize)
            {
                isDownloading = false;
                isComplete = true;
            }

            Console.WriteLine(downloadProgress.ToString() + "\r");
        }
        private List<Range> CalcRange()
        {
            var range = new List<Range>();
            long start = 0;
            if (totalSize > 0)
            {
                long average_size = totalSize / TotalThread;
                for (int i = 0; i < TotalThread; i++)
                {
                    if (i == TotalThread - 1)
                    {
                        range.Add(new Range
                        {
                            from = start,
                            to = totalSize
                        });
                    }
                    else
                    {
                        range.Add(new Range
                        {
                            from = start,
                            to = start + average_size
                        });
                        start += average_size;
                    }
                }
            }
            return range;
        }
        /// <summary>
        /// block thread to pause
        /// </summary>
        public void Pause()
        {
            resetEvent.Reset();
            isPaused = true;
            CalcProgress();
            CallBack?.Invoke();

        }

        /// <summary>
        /// release resetEvent to cntinue
        /// </summary>
        public void Continue()
        {
            resetEvent.Set();
            isPaused = false;
            CalcProgress();
            CallBack?.Invoke();

        }
        public void Cancel()
        {
            resetEvent.Set();
            isPaused = false;
            cts.Cancel();
            isComplete = true;
            CalcProgress();
            CallBack?.Invoke();

        }
        public DownloadStatus Status()
        {
            return new DownloadStatus
            {
                downloadProgress = downloadProgress,
                downloaded = downloaded,
                totalSize = totalSize,
                linkCount = linkCount,
                downloadingThread = downloadingThread,
                downloadBlockSize = downloadBlockSize,

                isPaused = isPaused,
                isDownloading = isDownloading,
                isComplete = isComplete,
            };
        }
        public async Task DownloadThread(Range range, string url)
        {

            long pointer = range.from;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "identity");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "zh-CN");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", "0");
                    if (Cookie != null)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", Cookie);
                    }
                    client.DefaultRequestHeaders.Add("Range", $"bytes={range.from}-{range.to}");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);

                    client.Timeout = TimeSpan.FromSeconds(10);
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        try
                        {
                            response.EnsureSuccessStatusCode();

                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(response.StatusCode);
                            throw ex;
                        }

                        resetEvent.WaitOne();
                        if (cts.IsCancellationRequested)
                        {
                            releaseFile();
                            // exit thread
                            return;
                        }
                        long? totalSize = response.Content.Headers.ContentLength;
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[BufferSize];
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                resetEvent.WaitOne();
                                if (cts.IsCancellationRequested)
                                {
                                    releaseFile();
                                    // exit thread
                                    return;
                                }
                                //await write 
                                downloaded += bytesRead;
                                lock (_lock)
                                {
                                    CalcProgress();
                                    Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        CallBack?.Invoke(); // 在 UI 线程中调用
                                    });
                                    FileStream.Seek(pointer, SeekOrigin.Begin);
                                    FileStream.Write(buffer, 0, bytesRead);
                                }
                                pointer += bytesRead;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                await DownloadThread(range, url);
            }

        }
        private bool isFileReleased()
        {
            try
            {
                return FileStream.SafeFileHandle.IsClosed;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }
        public void releaseFile()
        {
            lock (_lock)
            {
                if (!isFileReleased())
                {
                    FileStream.Close();

                }
            }
            if (cts.IsCancellationRequested)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            isDownloading = false;
        }
        public async void Run()
        {
            List<Range> rangeLsit = CalcRange();
            SemaphoreSlim semaphore = new SemaphoreSlim(TotalThread);
            Console.WriteLine(rangeLsit.Count.ToString());
            Task[] tasks = new Task[rangeLsit.Count];
            for (int i = 0; i < rangeLsit.Count; i++)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }
                await semaphore.WaitAsync();
                int threadId = i;
                int arrangeLinkId = threadId % linkList.Count;
                tasks[threadId] = Task.Run(async () =>
                {
                    try
                    {
                        downloadingThread++;
                        await DownloadThread(rangeLsit[threadId], linkList[arrangeLinkId]);
                        downloadingThread--;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
            if (!tasks.Contains(null))
            {
                await Task.WhenAll(tasks);

            }
            releaseFile();
            CalcProgress();
            CallBack?.Invoke();

        }

    }

}