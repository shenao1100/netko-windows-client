using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Netko.Download
{
    

    public class Downloader: IDownload
    {
        // 锁
        private readonly object _lock = new object();
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

        private float _downloadProgress = 0;
        private long _downloaded = 0;
        private long _totalSize = 0;
        private int _linkCount = 0;
        private int _downloadingThread = 0;
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

        private List<Range> CalcRange()
        {
            List<Range> ranges = new List<Range>();
            long calcedSize = 0;
            while (true)
            {
                if (calcedSize + _downloadBlockSize > _totalSize)
                {
                    ranges.Add(new Range
                    {
                        From = calcedSize,
                        To = _totalSize
                    });
                    break;

                }
                else
                {
                    ranges.Add(new Range
                    {
                        From = calcedSize,
                        To = calcedSize + _downloadBlockSize,
                    });
                }
                calcedSize += _downloadBlockSize + 1;

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

        private void Report()
        {
            if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastReportTime > 1)
            {
                CalcProgress();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    CallBack?.Invoke(); // 在 UI 线程中调用
                });
                lastReportTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
        public async Task DownloadThread(Range range, string url)
        {
            bool isCounted = false;
            long pointer = range.From;
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
                            throw ex;
                        }


                        _resetEvent.WaitOne();
                        
                        long? totalSize = response.Content.Headers.ContentLength;
                        
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[_bufferSize];
                            int bytesRead;
                            
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                _resetEvent.WaitOne();

                                if (_cts.IsCancellationRequested)
                                {
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

                            }
                            
                        }
                    }
                    if (isCounted)
                    {
                        _downloadingThread--;
                        isCounted = false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (isCounted)
                {
                    _downloadingThread--;
                    //isCounted = false;

                }
                await Task.Delay(500);
                Trace.WriteLine(ex.ToString());
                await DownloadThread(range, url);
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
            _runed = true;
            _isParsing = true;
            await UpdateDownloadUrl();
            _isParsing = false;
            List<Range> rangeLsit = CalcRange();
            SemaphoreSlim semaphore = new SemaphoreSlim(TotalThread);
            Task[] tasks = new Task[rangeLsit.Count];
            for (int i = 0; i < rangeLsit.Count; i++)
            {
                if (_cts.IsCancellationRequested)
                {
                    break;
                }
                await semaphore.WaitAsync();
                int threadId = i;
                int arrangeLinkId = threadId % _linkList.Count;
                tasks[threadId] = Task.Run(async () =>
                {
                    try
                    {
                        await DownloadThread(rangeLsit[threadId], _linkList[arrangeLinkId]);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                //await Task.Delay(300);

            }
            if (!tasks.Contains(null))
            {
                await Task.WhenAll(tasks);

            }
            ReleaseFile();
            CalcProgress();
            CallBack?.Invoke(); 

        }

    }

}