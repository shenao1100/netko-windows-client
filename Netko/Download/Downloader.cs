﻿using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Netko.Download
{
    /// <summary>
    /// 使用此结构来分配下载位
    /// if IsCompleted证明此片段下载已完成，分配线程时应跳过这一片段
    /// if IsOccupied则证明有线程正在下载此片段
    /// </summary>
    struct ProgressDistribute
    {
        public Range Range;
        public bool IsCompleted;
        public bool IsOccupied;
    }
    
    /// <summary>
    /// 可以复用的异步信号协调
    /// </summary>
    public class ResetableTaskCompleteSource
    {
        private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        public Task WaitAsync => _tcs.Task;

        public void SetCompleted()
        {
            _tcs.TrySetResult(true);
        }

        public void Reset()
        {
            _tcs = new TaskCompletionSource<bool>();
        }
    }
    
    public class Downloader: IDownload
    {
        // 锁
        private readonly object _lock = new object();
        // 用于保证key一致性的锁
        private readonly object _keyLock = new object();
        // 阻塞
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(true);
        // 取消信号
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private FileStream FileStream { get; set; }
        private int TotalThread { get; set; }
        private int CurrentThread { get; set; }
        private string? Url { get; set; }
        private string UserAgent { get; set; }
        private string FilePath { get; set; }

        public Action? CallBack { get; set; }

        private Dictionary<int, string> _linkList = new Dictionary<int, string>();
        private Dictionary<int, ProgressDistribute> _ranges = new Dictionary<int, ProgressDistribute>();
        
        private float _downloadProgress = 0;
        private long _downloaded = 0;
        private long _totalSize = 0;
        private int _linkCount = 0;
        private int _downloadingThread = 0;
        private int _notCompletedBlock = 0;

        private long _downloadBlockSize = 500087;
        private int _bufferSize = 1000;
        //public long downloadBlockSize = 32767;

        private bool _isPaused = false;
        private bool _isDownloading = true;
        private bool _isComplete = false;
        private string? _cookie = null;
        private bool _runed = false;
        private bool _isParsing = false;
        private Func<Task<List<string>>>? getUrlFunc;
        private long lastReportTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        public Downloader(DownloadConfig config)
        {
            TotalThread = config.DownloadThread;
            CurrentThread = 0;
            _downloadBlockSize = config.BlockSize;
            _bufferSize = config.BufferSize;
            Url = config.Url;
            if (config.Url != null)
            {
                _linkList.Add(_linkCount, config.Url);
            }
            _linkCount++;
            getUrlFunc = config.GetUrlFunc;
            UserAgent = config.UserAgent;
            _totalSize = config.FileSize;
            FilePath = config.FilePath;
            _cookie = config.Cookie;    

            FileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream.SetLength(_totalSize);
        }
        public void AddUrl(string url)
        {
            _linkList.Add(_linkCount, url);
            _linkCount++;
        }
        public void SetCallBack(Action callBack)
        {
            CallBack = callBack;
        }

        private void CalcProgress()
        {
            if (_downloaded > 0)
            {
                _downloadProgress = (float)_downloaded / _totalSize;
            }

            if (_downloadingThread == 0)
            {
                _isDownloading = false;
            }
            else
            {
                _isDownloading = true;
            }

            if (_downloaded == _totalSize)
            {
                _isDownloading = false;
                _isComplete = true;
            }
        }

        private Dictionary<int, ProgressDistribute> CalcRange()
        {
            Dictionary<int, ProgressDistribute> ranges = new Dictionary<int, ProgressDistribute>();
            long calcedSize = 0;
            int blockId = 0;
            while (true)
            {
                if (calcedSize + _downloadBlockSize > _totalSize)
                {
                    ranges[blockId] = new ProgressDistribute
                    {
                        IsCompleted = false,
                        IsOccupied = false,
                        Range = new Range
                        {
                            From = calcedSize,
                            To = _totalSize
                        }
                    };
                    break;

                }
                else
                {
                    ranges[blockId] = new ProgressDistribute
                    {
                        IsCompleted = false,
                        IsOccupied = false,
                        Range = new Range
                        {
                            From = calcedSize,
                            To = calcedSize + _downloadBlockSize,
                        }
                    };
                }
                calcedSize += _downloadBlockSize + 1;
                blockId++;
            }
            return ranges;
        }
        /// <summary>
        /// block thread to pause
        /// </summary>
        public void Pause()
        {
            _resetEvent.Reset();
            _isPaused = true;
            CalcProgress();
            CallBack?.Invoke();
              

        }

        /// <summary>
        /// release resetEvent to cntinue
        /// </summary>
        public void Continue()
        {
            _resetEvent.Set();
            _isPaused = false;
            CalcProgress();
            CallBack?.Invoke();
            if (!_runed)
            {
                Task.Run(() => { Run(); } );
            }

        }
        public void Cancel()
        {
            _resetEvent.Set();
            _isPaused = false;
            _cts.Cancel();
            _isComplete = true;
            CalcProgress();
            CallBack?.Invoke();
            ReleaseFile();


        }
        public DownloadStatus Status()
        {
            return new DownloadStatus
            {
                DownloadProgress = _downloadProgress,
                Downloaded = _downloaded,
                TotalSize = _totalSize,
                LinkCount = _linkCount,
                DownloadingThread = _downloadingThread,
                DownloadBlockSize = _downloadBlockSize,

                IsParsing = _isParsing,
                IsPaused = _isPaused,
                IsDownloading = _isDownloading,
                IsComplete = _isComplete,
            };
        }

        private void Report(bool forceReport = false)
        {
            CalcProgress();
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastReportTime > 1 || forceReport)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CallBack?.Invoke(); // 在 UI 线程中调用
                });
                lastReportTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
        public async Task DownloadThread(Range range, string url, int blockId)
        {
            bool isCounted = false;
            long pointer = range.From;
            ProgressDistribute tmpRange = _ranges[blockId];
            tmpRange.IsOccupied = true;
            _ranges[blockId] = tmpRange;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "identity");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "zh-CN");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", "0");
                    if (_cookie != null)
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", _cookie);
                    }
                    client.DefaultRequestHeaders.Add("Range", $"bytes={range.From}-{range.To}");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);

                    //client.Timeout = TimeSpan.FromSeconds(10);
                    _downloadingThread++;
                    isCounted = true;
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        if (_cts.IsCancellationRequested)
                        {
                            ReleaseFile();
                            // exit thread
                            return;
                        }
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception ex)
                        {
                            tmpRange = _ranges[blockId];
                            tmpRange.IsOccupied = false;
                            _ranges[blockId] = tmpRange;
                            return;
                            
                        }
                        _resetEvent.WaitOne();
                        /*if (_isPaused)
                        {
                            tmpRange = _ranges[blockId];
                            tmpRange.IsOccupied = false;
                            _ranges[blockId] = tmpRange;
                            return;
                        }*/
                        long? totalSize = response.Content.Headers.ContentLength;
                        
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[_bufferSize];
                            int bytesRead;
                            
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                _resetEvent.WaitOne();
                                /*if (_isPaused)
                                {
                                    tmpRange = _ranges[blockId];
                                    tmpRange.IsOccupied = false;
                                    _ranges[blockId] = tmpRange;
                                    return;
                                }*/
                                if (_cts.IsCancellationRequested)
                                {
                                    tmpRange = _ranges[blockId];
                                    tmpRange.IsOccupied = false;
                                    _ranges[blockId] = tmpRange;
                                    ReleaseFile();
                                    // exit thread
                                    return;
                                }
                                //await write 
                                _downloaded += bytesRead;
                                lock (_lock)
                                {
                                    Report();
                                    FileStream.Seek(pointer, SeekOrigin.Begin);
                                    FileStream.Write(buffer, 0, bytesRead);
                                }
                                pointer += bytesRead;
                                _ranges[blockId].Range.From += bytesRead;
                            }
                            
                        }
                    }
                    if (isCounted)
                    {
                        _downloadingThread--;
                        isCounted = false;
                    }
                    // 下载正常完毕
                    _notCompletedBlock--;
                    tmpRange = _ranges[blockId];
                    tmpRange.IsOccupied = false;
                    tmpRange.IsCompleted = true;
                    _ranges[blockId] = tmpRange;
                    return;
                }
            }
            catch (Exception ex)
            {
                if (isCounted)
                {
                    _downloadingThread--;
                }
                tmpRange = _ranges[blockId];
                tmpRange.IsOccupied = false;
                _ranges[blockId] = tmpRange;
                return;
                //await Task.Delay(500);
                //Trace.WriteLine(ex.ToString());
                //await DownloadThread(range, url);
            }

        }
        private bool IsFileReleased() {
            try
            {
                return FileStream.SafeFileHandle.IsClosed;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }
        public void ReleaseFile()
        {
            lock (_lock)
            {
                if (!IsFileReleased())
                {
                    FileStream.Close();

                }
            }
            if (_cts.IsCancellationRequested)
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            _isDownloading = false;
        }

        public async Task<bool> UpdateDownloadUrl()
        {
            if (getUrlFunc != null)
            {
                _linkList.Clear();
                _linkCount = 0;
                foreach (string url in await getUrlFunc())
                {
                    _linkList.Add(_linkCount, url);
                    _linkCount++;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        public async void Run()
        {
            int clearCounter = 0;
            int clearCounterMaxValue = 30;
            _runed = true;
            _isParsing = true;
            //await UpdateDownloadUrl();
            _isParsing = false;
            _ranges = CalcRange();
            _notCompletedBlock = _ranges.Count;
            // 信号量
            SemaphoreSlim semaphore = new SemaphoreSlim(TotalThread);
            // 异步信号协调
            // ResetableTaskCompleteSource tcs = new ResetableTaskCompleteSource();
            Task[] tasks = new Task[TotalThread];
            while (!_isComplete)
            {
                //Console.WriteLine("LOOP" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
                if (_cts.IsCancellationRequested || _isComplete)
                {
                    break;
                }

                clearCounter++;
                if (clearCounter >= clearCounterMaxValue)
                {
                    lock (_keyLock)
                    {
                        List<int> rangeKeyList = _ranges.Keys.ToList();
                        foreach (int blockId in rangeKeyList)
                        {
                            if (_ranges[blockId].IsCompleted)
                            {
                                _ranges.Remove(blockId);
                            }
                        }
                        clearCounter = 0;
                    }
                    // 剔除已经下载完成的块
                    
                }
                //等待线程空余
                await semaphore.WaitAsync();

                lock (_keyLock)
                {
                    _resetEvent.WaitOne();

                    // 遍历块
                    foreach (int j in _ranges.Keys.ToList())
                    {
                        // 若此块正在被下载或已经完成，则跳过
                        if (_ranges[j].IsCompleted || _ranges[j].IsOccupied){continue;}
                        //if (_ranges[j].IsOccupied || ) { continue; }

                        if (_cts.IsCancellationRequested || _isComplete) { break; }

                        // 遍历线程
                        for (int i = 0; i < TotalThread; i++)
                        {
                            if (_cts.IsCancellationRequested)
                            {
                                break;
                            }

                            // 线程正在使用则跳过
                            if (tasks[i] != null && !tasks[i].IsCompleted)
                            {
                                continue;
                            }
                            _resetEvent.WaitOne();

                            int threadId = i;
                            int blockId = j;
                            int arrangeLinkId = threadId % _linkList.Count;
                            tasks[threadId] = Task.Run(async () =>
                            {
                                try
                                {
                                    await DownloadThread(_ranges[blockId].Range, _linkList[arrangeLinkId], blockId);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            });
                            break;
                            //await Task.Delay(300);
                        }

                    }
                }
                
                // 剩余块数小于线程数

                if (!tasks.Contains(null) && _notCompletedBlock < TotalThread)
                {
                    await Task.WhenAll(tasks);
                }

            }
            //Console.WriteLine("Download Completed");
            ReleaseFile();
            CalcProgress();
            CallBack?.Invoke(); 

        }

    }

}