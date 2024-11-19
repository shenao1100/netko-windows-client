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

    public class Downloader
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

        public Action<Downloader>? CallBack { get; set; }

        private Dictionary<int, string> linkList = new Dictionary<int, string>();

        public float downloadProgress = 0;
        public long downloaded = 0;
        public long totalSize = 0;
        public int linkCount = 0;
        public int downloadingThread = 0;
        public long downloadBlockSize = 500087;

        public bool isPaused = false;
        public bool isDownloading = true;
        public bool isComplete = false;
        public Downloader(string url, string user_agent, string file_path, long total_size, int thread)
        {
            TotalThread = thread;
            CurrentThread = 0;
            Url = url;
            linkList.Add(linkCount, url);
            linkCount++;

            UserAgent = user_agent;
            totalSize = total_size;
            filePath = file_path;

            FileStream = new FileStream(file_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream.SetLength(total_size);
        }
        public void AddUrl(string url)
        {
            linkList.Add(linkCount, url);
            linkCount++;

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
            List<Range> ranges = new List<Range>();
            long calced_size = 0;
            while (true)
            {
                if (calced_size + downloadBlockSize > totalSize)
                {
                    ranges.Add(new Range
                    {
                        from = calced_size,
                        to = totalSize
                    });
                    break;

                }
                else
                {
                    ranges.Add(new Range
                    {
                        from = calced_size,
                        to = calced_size + downloadBlockSize,
                    });
                }
                calced_size += downloadBlockSize + 1;

            }
            return ranges;
        }
        /// <summary>
        /// block thread to pause
        /// </summary>
        public void Pause()
        {
            resetEvent.Reset();
            isPaused = true;
            CalcProgress();
            CallBack?.Invoke(this);

        }

        /// <summary>
        /// release resetEvent to cntinue
        /// </summary>
        public void Continue()
        {
            resetEvent.Set();
            isPaused = false;
            CalcProgress();
            CallBack?.Invoke(this);

        }
        public void Cancel()
        {
            resetEvent.Set();
            isPaused = false;
            cts.Cancel();
            isComplete = true;
            CalcProgress();
            CallBack?.Invoke(this);

        }
        public async Task DownloadThread(Range range, string url)
        {

            long pointer = range.from;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Range", $"bytes={range.from}-{range.to}");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);

                    client.Timeout = TimeSpan.FromSeconds(10);
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        response.EnsureSuccessStatusCode();
                        resetEvent.WaitOne();
                        if (cts.IsCancellationRequested)
                        {
                            // exit thread
                            return;
                        }
                        long? totalSize = response.Content.Headers.ContentLength;
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[1000];
                            int bytesRead;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                resetEvent.WaitOne();

                                //await write 
                                downloaded += bytesRead;
                                lock (_lock)
                                {
                                    Console.WriteLine("WriteTo: " + pointer.ToString() + " By: " + range.from.ToString());
                                    CalcProgress();
                                    Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        CallBack?.Invoke(this); // 在 UI 线程中调用
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
                await DownloadThread(range, url);
            }

        }
        public void releaseFile()
        {
            lock (_lock)
            {
                FileStream.Close();
            }
            if (cts.IsCancellationRequested)
            {
                File.Delete(filePath);   
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
            CallBack?.Invoke(this); 

        }

    }

}